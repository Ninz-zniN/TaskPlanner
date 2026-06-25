--
-- PostgreSQL database dump
--

\restrict BtKdyD5oa08fDa3YDNi3lj88USCohHiueVsQIBy8X57z3T3Jk3IUgym67jZmOaN

-- Dumped from database version 16.10
-- Dumped by pg_dump version 16.10

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: add_work_hours(timestamp without time zone, numeric, numeric); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.add_work_hours(p_start timestamp without time zone, p_hours numeric, p_hours_per_day numeric) RETURNS timestamp without time zone
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_remaining   NUMERIC := p_hours;
    v_current     TIMESTAMP := p_start;
    v_work_start  TIMESTAMP;
    v_work_end    TIMESTAMP;
    v_today_hours NUMERIC;
BEGIN
    WHILE v_remaining > 0 LOOP
        -- Выходные (0=Вс, 6=Сб) → переходим на следующий день
        IF EXTRACT(DOW FROM v_current) IN (0, 6) THEN
            v_current := date_trunc('day', v_current) + INTERVAL '1 day' + INTERVAL '9 hours';
            CONTINUE;
        END IF;

        v_work_start := date_trunc('day', v_current) + INTERVAL '9 hours';
        v_work_end   := v_work_start + (p_hours_per_day || ' hours')::INTERVAL;

        -- Если мы раньше начала рабочего дня → сдвигаемся на 9:00
        IF v_current < v_work_start THEN
            v_current := v_work_start;
        END IF;

        -- Если уже позже конца рабочего дня → переходим на следующий день
        IF v_current >= v_work_end THEN
            v_current := date_trunc('day', v_current) + INTERVAL '1 day' + INTERVAL '9 hours';
            CONTINUE;
        END IF;

        -- Сколько часов осталось сегодня
        v_today_hours := EXTRACT(EPOCH FROM (v_work_end - v_current)) / 3600;
        IF v_today_hours > v_remaining THEN
            v_today_hours := v_remaining;
        END IF;

        v_remaining := v_remaining - v_today_hours;
        v_current   := v_current + (v_today_hours || ' hours')::INTERVAL;

        -- Если ещё остались часы → переходим на начало следующего рабочего дня
        IF v_remaining > 0 THEN
            v_current := date_trunc('day', v_current) + INTERVAL '1 day' + INTERVAL '9 hours';
        END IF;
    END LOOP;

    RETURN v_current;
END;
$$;


ALTER FUNCTION public.add_work_hours(p_start timestamp without time zone, p_hours numeric, p_hours_per_day numeric) OWNER TO postgres;

--
-- Name: generate_task_history(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.generate_task_history(p_task_id integer) RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_start          TIMESTAMP;
    v_assignee_id    INT;
    v_current_assignee_id INT;
    v_team_id        INT;
    v_hours_per_day  NUMERIC;
    v_estimate       NUMERIC;
    v_work_hours     NUMERIC;
    v_review_hours   NUMERIC;
    v_test_hours     NUMERIC;
    v_current_status INT;
    v_old_status     INT;
    v_next_date      TIMESTAMP;
    v_max_date       TIMESTAMP;
    v_is_finished    BOOLEAN := FALSE;
    v_statuses       INT[] := ARRAY[2, 3, 4, 5];
    v_status         INT;
    v_hours          NUMERIC;
    v_random_factor  NUMERIC;
    v_sprint_end     TIMESTAMP;
    v_sprint_start   TIMESTAMP;
    v_sprint_id      INT;
    v_total_days     INT;
    v_log_id         INT;
BEGIN
    SELECT t.created_at, t.id_assignee, t.estimate_hours, t.id_task_status, t.id_sprint, e.id_team
    INTO v_start, v_assignee_id, v_estimate, v_current_status, v_sprint_id, v_team_id
    FROM Task t LEFT JOIN Employee e ON t.id_assignee = e.id_employee
    WHERE t.id_task = p_task_id;
    IF v_start IS NULL OR v_assignee_id IS NULL OR v_estimate <= 0 THEN RETURN; END IF;

    SELECT hours_per_day INTO v_hours_per_day FROM Employee WHERE id_employee = v_assignee_id;
    v_hours_per_day := COALESCE(v_hours_per_day, 8);

    IF v_sprint_id IS NOT NULL THEN
        SELECT start_date, end_date INTO v_sprint_start, v_sprint_end FROM Sprint WHERE id_sprint = v_sprint_id;
        IF v_sprint_end IS NOT NULL THEN
            IF v_sprint_end <= NOW() THEN v_max_date := v_sprint_end; v_is_finished := TRUE;
            ELSE v_max_date := NOW(); v_is_finished := FALSE; END IF;
        ELSE v_max_date := NOW(); END IF;
    ELSE v_max_date := NOW(); v_is_finished := FALSE; END IF;

    -- Начальная запись "Новая"
    INSERT INTO Task_History (id_task, old_status, new_status, changed_by, assignee_id, changed_at, actual_hours)
    VALUES (p_task_id, NULL, 1, 1, v_assignee_id, v_start, 0);

    v_random_factor := 0.9 + random() * 0.3;
    v_work_hours   := v_estimate * v_random_factor;
    v_review_hours := LEAST(v_estimate * (0.05 + random() * 0.15), 4);
    v_test_hours   := v_estimate * (0.05 + random() * 0.15);
    IF v_test_hours < 0.5 THEN v_test_hours := 0.5; END IF;

    -- Старт работы
    IF v_is_finished AND v_sprint_start IS NOT NULL THEN
        v_total_days := GREATEST(1, (v_sprint_end::date - v_sprint_start::date));
        v_next_date := v_sprint_start + (random() * (v_total_days - 1) || ' days')::INTERVAL;
        IF v_next_date < v_start THEN v_next_date := v_start + (random() * INTERVAL '4 hours'); END IF;
        IF v_next_date > v_sprint_end - INTERVAL '3 days' THEN v_next_date := v_sprint_end - INTERVAL '3 days'; END IF;
    ELSE
        v_next_date := v_start + (random() * INTERVAL '4 hours');
        IF v_next_date > NOW() THEN v_next_date := NOW(); END IF;
    END IF;

    v_current_assignee_id := v_assignee_id;
    v_old_status := 1;

    FOREACH v_status IN ARRAY v_statuses LOOP
        -- Для статуса "Готово" в завершённом спринте подгоняем дату под конец
        IF v_is_finished AND v_status = 5 AND v_next_date > v_max_date THEN
            v_next_date := v_max_date - (random() * INTERVAL '1 hour');
        END IF;
        IF v_next_date > v_max_date THEN EXIT; END IF;

        CASE v_status
            WHEN 2 THEN
                v_hours := 0;
                v_next_date := v_next_date + (random() * INTERVAL '5 minutes'); -- чтобы не совпадало с предыдущей записью
            WHEN 3 THEN
                -- Основное время работы: сначала сдвигаем дату, потом вставляем
                v_next_date := add_work_hours(v_next_date, v_work_hours, v_hours_per_day);
                v_hours := v_work_hours;
                v_current_assignee_id := get_random_team_member(v_assignee_id, v_team_id);
            WHEN 4 THEN
                v_next_date := add_work_hours(v_next_date, v_review_hours, v_hours_per_day);
                v_hours := v_review_hours;
                v_current_assignee_id := get_random_team_member(v_assignee_id, v_team_id);
            WHEN 5 THEN
                v_next_date := add_work_hours(v_next_date, v_test_hours, v_hours_per_day);
                v_hours := v_test_hours;
        END CASE;

        INSERT INTO Task_History (id_task, old_status, new_status, changed_by, assignee_id, changed_at, actual_hours)
        VALUES (p_task_id, v_old_status, v_status, 1, v_current_assignee_id, v_next_date, v_hours)
        RETURNING id_log INTO v_log_id;

        v_old_status := v_status;
        v_current_status := v_status;
    END LOOP;

    IF v_current_status != (SELECT id_task_status FROM Task WHERE id_task = p_task_id) THEN
        UPDATE Task SET id_task_status = v_current_status, updated_at = NOW() WHERE id_task = p_task_id;
    END IF;
END;
$$;


ALTER FUNCTION public.generate_task_history(p_task_id integer) OWNER TO postgres;

--
-- Name: get_random_team_member(integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.get_random_team_member(p_employee_id integer, p_team_id integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_id INT;
BEGIN
    IF p_team_id IS NULL THEN RETURN p_employee_id; END IF;
    
    SELECT e.id_employee INTO v_id
    FROM Employee e
    WHERE e.id_team = p_team_id AND e.id_employee != p_employee_id
    ORDER BY random() LIMIT 1;
    
    RETURN COALESCE(v_id, p_employee_id);
END;
$$;


ALTER FUNCTION public.get_random_team_member(p_employee_id integer, p_team_id integer) OWNER TO postgres;

--
-- Name: prevent_manual_actual_hours_update(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.prevent_manual_actual_hours_update() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF OLD.actual_hours IS DISTINCT FROM NEW.actual_hours THEN
        RAISE EXCEPTION 'Ручное изменение actual_hours запрещено. Поле вычисляется автоматически.';
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.prevent_manual_actual_hours_update() OWNER TO postgres;

--
-- Name: update_task_actual_hours(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_task_actual_hours() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    -- Временно отключаем защитный триггер, чтобы наше обновление прошло
    SET LOCAL session_replication_role = 'replica';
    
    UPDATE Task
    SET actual_hours = (
        SELECT COALESCE(SUM(th.actual_hours), 0)
        FROM Task_History th
        WHERE th.id_task = COALESCE(NEW.id_task, OLD.id_task)
    )
    WHERE id_task = COALESCE(NEW.id_task, OLD.id_task);
    
    RETURN NULL;
END;
$$;


ALTER FUNCTION public.update_task_actual_hours() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: employee; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.employee (
    id_employee integer NOT NULL,
    last_name text NOT NULL,
    first_name text NOT NULL,
    patronymic text,
    "position" text,
    grade text,
    id_team integer,
    hours_per_day numeric(3,1) DEFAULT 6 NOT NULL,
    is_dismissed boolean DEFAULT false,
    CONSTRAINT employee_grade_check CHECK ((grade = ANY (ARRAY['Intern'::text, 'Junior'::text, 'Middle'::text, 'Senior'::text, 'Lead'::text])))
);


ALTER TABLE public.employee OWNER TO postgres;

--
-- Name: employee_id_employee_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.employee_id_employee_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.employee_id_employee_seq OWNER TO postgres;

--
-- Name: employee_id_employee_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.employee_id_employee_seq OWNED BY public.employee.id_employee;


--
-- Name: employee_project; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.employee_project (
    id_employee integer NOT NULL,
    id_project integer NOT NULL,
    assigned_date date DEFAULT CURRENT_DATE
);


ALTER TABLE public.employee_project OWNER TO postgres;

--
-- Name: priority; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.priority (
    id_priority integer NOT NULL,
    priority_name text NOT NULL,
    priority_weight integer NOT NULL,
    CONSTRAINT priority_priority_weight_check CHECK (((priority_weight >= 0) AND (priority_weight <= 100)))
);


ALTER TABLE public.priority OWNER TO postgres;

--
-- Name: priority_id_priority_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.priority_id_priority_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.priority_id_priority_seq OWNER TO postgres;

--
-- Name: priority_id_priority_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.priority_id_priority_seq OWNED BY public.priority.id_priority;


--
-- Name: project; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.project (
    id_project integer NOT NULL,
    project_name text NOT NULL,
    description text,
    status text DEFAULT 'planning'::text,
    id_manager integer,
    id_team integer,
    completed_at timestamp without time zone,
    CONSTRAINT project_status_check CHECK ((status = ANY (ARRAY['planning'::text, 'active'::text, 'completed'::text])))
);


ALTER TABLE public.project OWNER TO postgres;

--
-- Name: project_id_project_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.project_id_project_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.project_id_project_seq OWNER TO postgres;

--
-- Name: project_id_project_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.project_id_project_seq OWNED BY public.project.id_project;


--
-- Name: sprint; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sprint (
    id_sprint integer NOT NULL,
    sprint_name text NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    work_days integer DEFAULT 10 NOT NULL,
    is_active boolean DEFAULT false,
    CONSTRAINT sprint_work_days_check CHECK ((work_days > 0))
);


ALTER TABLE public.sprint OWNER TO postgres;

--
-- Name: sprint_id_sprint_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sprint_id_sprint_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.sprint_id_sprint_seq OWNER TO postgres;

--
-- Name: sprint_id_sprint_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sprint_id_sprint_seq OWNED BY public.sprint.id_sprint;


--
-- Name: task; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.task (
    id_task integer NOT NULL,
    title text NOT NULL,
    description text,
    id_task_type integer,
    id_task_status integer,
    id_priority integer,
    estimate_hours numeric(6,1) NOT NULL,
    actual_hours numeric(6,1),
    deadline date,
    id_assignee integer,
    id_project integer,
    id_sprint integer,
    created_by integer,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone,
    CONSTRAINT task_actual_hours_check CHECK ((actual_hours >= (0)::numeric)),
    CONSTRAINT task_estimate_hours_check CHECK ((estimate_hours >= (0)::numeric))
);


ALTER TABLE public.task OWNER TO postgres;

--
-- Name: task_history; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.task_history (
    id_log integer NOT NULL,
    id_task integer NOT NULL,
    old_status integer,
    new_status integer NOT NULL,
    changed_by integer NOT NULL,
    changed_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    assignee_id integer,
    actual_hours numeric(6,1)
);


ALTER TABLE public.task_history OWNER TO postgres;

--
-- Name: task_history_id_log_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.task_history_id_log_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.task_history_id_log_seq OWNER TO postgres;

--
-- Name: task_history_id_log_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.task_history_id_log_seq OWNED BY public.task_history.id_log;


--
-- Name: task_id_task_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.task_id_task_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.task_id_task_seq OWNER TO postgres;

--
-- Name: task_id_task_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.task_id_task_seq OWNED BY public.task.id_task;


--
-- Name: task_note; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.task_note (
    id_note integer NOT NULL,
    id_task integer NOT NULL,
    author_id integer NOT NULL,
    content text NOT NULL,
    created_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.task_note OWNER TO postgres;

--
-- Name: task_note_id_note_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.task_note_id_note_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.task_note_id_note_seq OWNER TO postgres;

--
-- Name: task_note_id_note_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.task_note_id_note_seq OWNED BY public.task_note.id_note;


--
-- Name: task_status; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.task_status (
    id_task_status integer NOT NULL,
    status_name text NOT NULL
);


ALTER TABLE public.task_status OWNER TO postgres;

--
-- Name: task_status_id_task_status_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.task_status_id_task_status_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.task_status_id_task_status_seq OWNER TO postgres;

--
-- Name: task_status_id_task_status_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.task_status_id_task_status_seq OWNED BY public.task_status.id_task_status;


--
-- Name: task_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.task_type (
    id_task_type integer NOT NULL,
    type_name text NOT NULL
);


ALTER TABLE public.task_type OWNER TO postgres;

--
-- Name: task_type_id_task_type_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.task_type_id_task_type_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.task_type_id_task_type_seq OWNER TO postgres;

--
-- Name: task_type_id_task_type_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.task_type_id_task_type_seq OWNED BY public.task_type.id_task_type;


--
-- Name: team; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.team (
    id_team integer NOT NULL,
    team_name text NOT NULL,
    team_lead_id integer
);


ALTER TABLE public.team OWNER TO postgres;

--
-- Name: team_id_team_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.team_id_team_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.team_id_team_seq OWNER TO postgres;

--
-- Name: team_id_team_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.team_id_team_seq OWNED BY public.team.id_team;


--
-- Name: user_role; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.user_role (
    id_role integer NOT NULL,
    role_name text NOT NULL
);


ALTER TABLE public.user_role OWNER TO postgres;

--
-- Name: user_role_id_role_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.user_role_id_role_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.user_role_id_role_seq OWNER TO postgres;

--
-- Name: user_role_id_role_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.user_role_id_role_seq OWNED BY public.user_role.id_role;


--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    id_user integer NOT NULL,
    login character varying(100) NOT NULL,
    password_hash text NOT NULL,
    id_role integer NOT NULL,
    id_employee integer,
    is_active boolean DEFAULT true,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    last_login timestamp without time zone
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Name: users_id_user_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_id_user_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_id_user_seq OWNER TO postgres;

--
-- Name: users_id_user_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_id_user_seq OWNED BY public.users.id_user;


--
-- Name: employee id_employee; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employee ALTER COLUMN id_employee SET DEFAULT nextval('public.employee_id_employee_seq'::regclass);


--
-- Name: priority id_priority; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.priority ALTER COLUMN id_priority SET DEFAULT nextval('public.priority_id_priority_seq'::regclass);


--
-- Name: project id_project; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.project ALTER COLUMN id_project SET DEFAULT nextval('public.project_id_project_seq'::regclass);


--
-- Name: sprint id_sprint; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sprint ALTER COLUMN id_sprint SET DEFAULT nextval('public.sprint_id_sprint_seq'::regclass);


--
-- Name: task id_task; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task ALTER COLUMN id_task SET DEFAULT nextval('public.task_id_task_seq'::regclass);


--
-- Name: task_history id_log; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_history ALTER COLUMN id_log SET DEFAULT nextval('public.task_history_id_log_seq'::regclass);


--
-- Name: task_note id_note; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_note ALTER COLUMN id_note SET DEFAULT nextval('public.task_note_id_note_seq'::regclass);


--
-- Name: task_status id_task_status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_status ALTER COLUMN id_task_status SET DEFAULT nextval('public.task_status_id_task_status_seq'::regclass);


--
-- Name: task_type id_task_type; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_type ALTER COLUMN id_task_type SET DEFAULT nextval('public.task_type_id_task_type_seq'::regclass);


--
-- Name: team id_team; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.team ALTER COLUMN id_team SET DEFAULT nextval('public.team_id_team_seq'::regclass);


--
-- Name: user_role id_role; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_role ALTER COLUMN id_role SET DEFAULT nextval('public.user_role_id_role_seq'::regclass);


--
-- Name: users id_user; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN id_user SET DEFAULT nextval('public.users_id_user_seq'::regclass);


--
-- Name: employee employee_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employee
    ADD CONSTRAINT employee_pkey PRIMARY KEY (id_employee);


--
-- Name: employee_project employee_project_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employee_project
    ADD CONSTRAINT employee_project_pkey PRIMARY KEY (id_employee, id_project);


--
-- Name: priority priority_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.priority
    ADD CONSTRAINT priority_pkey PRIMARY KEY (id_priority);


--
-- Name: priority priority_priority_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.priority
    ADD CONSTRAINT priority_priority_name_key UNIQUE (priority_name);


--
-- Name: project project_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.project
    ADD CONSTRAINT project_pkey PRIMARY KEY (id_project);


--
-- Name: sprint sprint_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sprint
    ADD CONSTRAINT sprint_pkey PRIMARY KEY (id_sprint);


--
-- Name: task_history task_history_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_history
    ADD CONSTRAINT task_history_pkey PRIMARY KEY (id_log);


--
-- Name: task_note task_note_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_note
    ADD CONSTRAINT task_note_pkey PRIMARY KEY (id_note);


--
-- Name: task task_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task
    ADD CONSTRAINT task_pkey PRIMARY KEY (id_task);


--
-- Name: task_status task_status_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_status
    ADD CONSTRAINT task_status_pkey PRIMARY KEY (id_task_status);


--
-- Name: task_status task_status_status_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_status
    ADD CONSTRAINT task_status_status_name_key UNIQUE (status_name);


--
-- Name: task_type task_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_type
    ADD CONSTRAINT task_type_pkey PRIMARY KEY (id_task_type);


--
-- Name: task_type task_type_type_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_type
    ADD CONSTRAINT task_type_type_name_key UNIQUE (type_name);


--
-- Name: team team_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.team
    ADD CONSTRAINT team_pkey PRIMARY KEY (id_team);


--
-- Name: team team_team_lead_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.team
    ADD CONSTRAINT team_team_lead_id_key UNIQUE (team_lead_id);


--
-- Name: user_role user_role_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_role
    ADD CONSTRAINT user_role_pkey PRIMARY KEY (id_role);


--
-- Name: user_role user_role_role_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_role
    ADD CONSTRAINT user_role_role_name_key UNIQUE (role_name);


--
-- Name: users users_id_employee_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_id_employee_key UNIQUE (id_employee);


--
-- Name: users users_login_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_login_key UNIQUE (login);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id_user);


--
-- Name: idx_employee_team; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_employee_team ON public.employee USING btree (id_team);


--
-- Name: idx_sprint_active; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sprint_active ON public.sprint USING btree (is_active) WHERE (is_active = true);


--
-- Name: idx_task_assignee; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_task_assignee ON public.task USING btree (id_assignee);


--
-- Name: idx_task_deadline; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_task_deadline ON public.task USING btree (deadline) WHERE (id_task_status <> ALL (ARRAY[5, 6]));


--
-- Name: idx_task_note_task; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_task_note_task ON public.task_note USING btree (id_task);


--
-- Name: idx_task_project; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_task_project ON public.task USING btree (id_project);


--
-- Name: idx_task_sprint; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_task_sprint ON public.task USING btree (id_sprint);


--
-- Name: idx_task_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_task_status ON public.task USING btree (id_task_status);


--
-- Name: idx_task_updated; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_task_updated ON public.task USING btree (updated_at);


--
-- Name: idx_user_login; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_user_login ON public.users USING btree (login);


--
-- Name: task trg_prevent_manual_actual_hours; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trg_prevent_manual_actual_hours BEFORE UPDATE OF actual_hours ON public.task FOR EACH ROW EXECUTE FUNCTION public.prevent_manual_actual_hours_update();


--
-- Name: task_history trg_update_task_actual_hours; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trg_update_task_actual_hours AFTER INSERT OR DELETE OR UPDATE ON public.task_history FOR EACH ROW EXECUTE FUNCTION public.update_task_actual_hours();


--
-- Name: employee employee_id_team_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employee
    ADD CONSTRAINT employee_id_team_fkey FOREIGN KEY (id_team) REFERENCES public.team(id_team) ON DELETE SET NULL;


--
-- Name: employee_project employee_project_id_employee_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employee_project
    ADD CONSTRAINT employee_project_id_employee_fkey FOREIGN KEY (id_employee) REFERENCES public.employee(id_employee) ON DELETE CASCADE;


--
-- Name: employee_project employee_project_id_project_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employee_project
    ADD CONSTRAINT employee_project_id_project_fkey FOREIGN KEY (id_project) REFERENCES public.project(id_project) ON DELETE CASCADE;


--
-- Name: team fk_team_lead; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.team
    ADD CONSTRAINT fk_team_lead FOREIGN KEY (team_lead_id) REFERENCES public.employee(id_employee) ON DELETE SET NULL;


--
-- Name: project project_id_manager_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.project
    ADD CONSTRAINT project_id_manager_fkey FOREIGN KEY (id_manager) REFERENCES public.employee(id_employee) ON DELETE SET NULL;


--
-- Name: project project_id_team_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.project
    ADD CONSTRAINT project_id_team_fkey FOREIGN KEY (id_team) REFERENCES public.team(id_team) ON DELETE SET NULL;


--
-- Name: task task_created_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task
    ADD CONSTRAINT task_created_by_fkey FOREIGN KEY (created_by) REFERENCES public.users(id_user);


--
-- Name: task_history task_history_assignee_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_history
    ADD CONSTRAINT task_history_assignee_id_fkey FOREIGN KEY (assignee_id) REFERENCES public.employee(id_employee) ON DELETE SET NULL;


--
-- Name: task_history task_history_changed_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_history
    ADD CONSTRAINT task_history_changed_by_fkey FOREIGN KEY (changed_by) REFERENCES public.users(id_user);


--
-- Name: task_history task_history_id_task_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_history
    ADD CONSTRAINT task_history_id_task_fkey FOREIGN KEY (id_task) REFERENCES public.task(id_task) ON DELETE CASCADE;


--
-- Name: task_history task_history_new_status_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_history
    ADD CONSTRAINT task_history_new_status_fkey FOREIGN KEY (new_status) REFERENCES public.task_status(id_task_status);


--
-- Name: task_history task_history_old_status_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_history
    ADD CONSTRAINT task_history_old_status_fkey FOREIGN KEY (old_status) REFERENCES public.task_status(id_task_status);


--
-- Name: task task_id_assignee_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task
    ADD CONSTRAINT task_id_assignee_fkey FOREIGN KEY (id_assignee) REFERENCES public.employee(id_employee) ON DELETE SET NULL;


--
-- Name: task task_id_priority_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task
    ADD CONSTRAINT task_id_priority_fkey FOREIGN KEY (id_priority) REFERENCES public.priority(id_priority);


--
-- Name: task task_id_project_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task
    ADD CONSTRAINT task_id_project_fkey FOREIGN KEY (id_project) REFERENCES public.project(id_project) ON DELETE CASCADE;


--
-- Name: task task_id_sprint_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task
    ADD CONSTRAINT task_id_sprint_fkey FOREIGN KEY (id_sprint) REFERENCES public.sprint(id_sprint) ON DELETE SET NULL;


--
-- Name: task task_id_task_status_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task
    ADD CONSTRAINT task_id_task_status_fkey FOREIGN KEY (id_task_status) REFERENCES public.task_status(id_task_status);


--
-- Name: task task_id_task_type_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task
    ADD CONSTRAINT task_id_task_type_fkey FOREIGN KEY (id_task_type) REFERENCES public.task_type(id_task_type);


--
-- Name: task_note task_note_author_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_note
    ADD CONSTRAINT task_note_author_id_fkey FOREIGN KEY (author_id) REFERENCES public.users(id_user);


--
-- Name: task_note task_note_id_task_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.task_note
    ADD CONSTRAINT task_note_id_task_fkey FOREIGN KEY (id_task) REFERENCES public.task(id_task) ON DELETE CASCADE;


--
-- Name: users users_id_employee_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_id_employee_fkey FOREIGN KEY (id_employee) REFERENCES public.employee(id_employee) ON DELETE SET NULL;


--
-- Name: users users_id_role_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_id_role_fkey FOREIGN KEY (id_role) REFERENCES public.user_role(id_role);


--
-- PostgreSQL database dump complete
--

\unrestrict BtKdyD5oa08fDa3YDNi3lj88USCohHiueVsQIBy8X57z3T3Jk3IUgym67jZmOaN

