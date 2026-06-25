--
-- PostgreSQL database dump
--

\restrict h6cvQQOcYxPcfXotTUpOi6Ys0gEvddnRdSMsjVi060XSzccEmbGSnI1eQhVFx4g

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

ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS users_id_role_fkey;
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS users_id_employee_fkey;
ALTER TABLE IF EXISTS ONLY public.task_note DROP CONSTRAINT IF EXISTS task_note_id_task_fkey;
ALTER TABLE IF EXISTS ONLY public.task_note DROP CONSTRAINT IF EXISTS task_note_author_id_fkey;
ALTER TABLE IF EXISTS ONLY public.task DROP CONSTRAINT IF EXISTS task_id_task_type_fkey;
ALTER TABLE IF EXISTS ONLY public.task DROP CONSTRAINT IF EXISTS task_id_task_status_fkey;
ALTER TABLE IF EXISTS ONLY public.task DROP CONSTRAINT IF EXISTS task_id_sprint_fkey;
ALTER TABLE IF EXISTS ONLY public.task DROP CONSTRAINT IF EXISTS task_id_project_fkey;
ALTER TABLE IF EXISTS ONLY public.task DROP CONSTRAINT IF EXISTS task_id_priority_fkey;
ALTER TABLE IF EXISTS ONLY public.task DROP CONSTRAINT IF EXISTS task_id_assignee_fkey;
ALTER TABLE IF EXISTS ONLY public.task_history DROP CONSTRAINT IF EXISTS task_history_old_status_fkey;
ALTER TABLE IF EXISTS ONLY public.task_history DROP CONSTRAINT IF EXISTS task_history_new_status_fkey;
ALTER TABLE IF EXISTS ONLY public.task_history DROP CONSTRAINT IF EXISTS task_history_id_task_fkey;
ALTER TABLE IF EXISTS ONLY public.task_history DROP CONSTRAINT IF EXISTS task_history_changed_by_fkey;
ALTER TABLE IF EXISTS ONLY public.task_history DROP CONSTRAINT IF EXISTS task_history_assignee_id_fkey;
ALTER TABLE IF EXISTS ONLY public.task DROP CONSTRAINT IF EXISTS task_created_by_fkey;
ALTER TABLE IF EXISTS ONLY public.project DROP CONSTRAINT IF EXISTS project_id_team_fkey;
ALTER TABLE IF EXISTS ONLY public.project DROP CONSTRAINT IF EXISTS project_id_manager_fkey;
ALTER TABLE IF EXISTS ONLY public.team DROP CONSTRAINT IF EXISTS fk_team_lead;
ALTER TABLE IF EXISTS ONLY public.employee_project DROP CONSTRAINT IF EXISTS employee_project_id_project_fkey;
ALTER TABLE IF EXISTS ONLY public.employee_project DROP CONSTRAINT IF EXISTS employee_project_id_employee_fkey;
ALTER TABLE IF EXISTS ONLY public.employee DROP CONSTRAINT IF EXISTS employee_id_team_fkey;
DROP TRIGGER IF EXISTS trg_update_task_actual_hours ON public.task_history;
DROP TRIGGER IF EXISTS trg_prevent_manual_actual_hours ON public.task;
DROP INDEX IF EXISTS public.idx_user_login;
DROP INDEX IF EXISTS public.idx_task_updated;
DROP INDEX IF EXISTS public.idx_task_status;
DROP INDEX IF EXISTS public.idx_task_sprint;
DROP INDEX IF EXISTS public.idx_task_project;
DROP INDEX IF EXISTS public.idx_task_note_task;
DROP INDEX IF EXISTS public.idx_task_deadline;
DROP INDEX IF EXISTS public.idx_task_assignee;
DROP INDEX IF EXISTS public.idx_sprint_active;
DROP INDEX IF EXISTS public.idx_employee_team;
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS users_pkey;
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS users_login_key;
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS users_id_employee_key;
ALTER TABLE IF EXISTS ONLY public.user_role DROP CONSTRAINT IF EXISTS user_role_role_name_key;
ALTER TABLE IF EXISTS ONLY public.user_role DROP CONSTRAINT IF EXISTS user_role_pkey;
ALTER TABLE IF EXISTS ONLY public.team DROP CONSTRAINT IF EXISTS team_team_lead_id_key;
ALTER TABLE IF EXISTS ONLY public.team DROP CONSTRAINT IF EXISTS team_pkey;
ALTER TABLE IF EXISTS ONLY public.task_type DROP CONSTRAINT IF EXISTS task_type_type_name_key;
ALTER TABLE IF EXISTS ONLY public.task_type DROP CONSTRAINT IF EXISTS task_type_pkey;
ALTER TABLE IF EXISTS ONLY public.task_status DROP CONSTRAINT IF EXISTS task_status_status_name_key;
ALTER TABLE IF EXISTS ONLY public.task_status DROP CONSTRAINT IF EXISTS task_status_pkey;
ALTER TABLE IF EXISTS ONLY public.task DROP CONSTRAINT IF EXISTS task_pkey;
ALTER TABLE IF EXISTS ONLY public.task_note DROP CONSTRAINT IF EXISTS task_note_pkey;
ALTER TABLE IF EXISTS ONLY public.task_history DROP CONSTRAINT IF EXISTS task_history_pkey;
ALTER TABLE IF EXISTS ONLY public.sprint DROP CONSTRAINT IF EXISTS sprint_pkey;
ALTER TABLE IF EXISTS ONLY public.project DROP CONSTRAINT IF EXISTS project_pkey;
ALTER TABLE IF EXISTS ONLY public.priority DROP CONSTRAINT IF EXISTS priority_priority_name_key;
ALTER TABLE IF EXISTS ONLY public.priority DROP CONSTRAINT IF EXISTS priority_pkey;
ALTER TABLE IF EXISTS ONLY public.employee_project DROP CONSTRAINT IF EXISTS employee_project_pkey;
ALTER TABLE IF EXISTS ONLY public.employee DROP CONSTRAINT IF EXISTS employee_pkey;
ALTER TABLE IF EXISTS public.users ALTER COLUMN id_user DROP DEFAULT;
ALTER TABLE IF EXISTS public.user_role ALTER COLUMN id_role DROP DEFAULT;
ALTER TABLE IF EXISTS public.team ALTER COLUMN id_team DROP DEFAULT;
ALTER TABLE IF EXISTS public.task_type ALTER COLUMN id_task_type DROP DEFAULT;
ALTER TABLE IF EXISTS public.task_status ALTER COLUMN id_task_status DROP DEFAULT;
ALTER TABLE IF EXISTS public.task_note ALTER COLUMN id_note DROP DEFAULT;
ALTER TABLE IF EXISTS public.task_history ALTER COLUMN id_log DROP DEFAULT;
ALTER TABLE IF EXISTS public.task ALTER COLUMN id_task DROP DEFAULT;
ALTER TABLE IF EXISTS public.sprint ALTER COLUMN id_sprint DROP DEFAULT;
ALTER TABLE IF EXISTS public.project ALTER COLUMN id_project DROP DEFAULT;
ALTER TABLE IF EXISTS public.priority ALTER COLUMN id_priority DROP DEFAULT;
ALTER TABLE IF EXISTS public.employee ALTER COLUMN id_employee DROP DEFAULT;
DROP SEQUENCE IF EXISTS public.users_id_user_seq;
DROP TABLE IF EXISTS public.users;
DROP SEQUENCE IF EXISTS public.user_role_id_role_seq;
DROP TABLE IF EXISTS public.user_role;
DROP SEQUENCE IF EXISTS public.team_id_team_seq;
DROP TABLE IF EXISTS public.team;
DROP SEQUENCE IF EXISTS public.task_type_id_task_type_seq;
DROP TABLE IF EXISTS public.task_type;
DROP SEQUENCE IF EXISTS public.task_status_id_task_status_seq;
DROP TABLE IF EXISTS public.task_status;
DROP SEQUENCE IF EXISTS public.task_note_id_note_seq;
DROP TABLE IF EXISTS public.task_note;
DROP SEQUENCE IF EXISTS public.task_id_task_seq;
DROP SEQUENCE IF EXISTS public.task_history_id_log_seq;
DROP TABLE IF EXISTS public.task_history;
DROP TABLE IF EXISTS public.task;
DROP SEQUENCE IF EXISTS public.sprint_id_sprint_seq;
DROP TABLE IF EXISTS public.sprint;
DROP SEQUENCE IF EXISTS public.project_id_project_seq;
DROP TABLE IF EXISTS public.project;
DROP SEQUENCE IF EXISTS public.priority_id_priority_seq;
DROP TABLE IF EXISTS public.priority;
DROP TABLE IF EXISTS public.employee_project;
DROP SEQUENCE IF EXISTS public.employee_id_employee_seq;
DROP TABLE IF EXISTS public.employee;
DROP FUNCTION IF EXISTS public.update_task_actual_hours();
DROP FUNCTION IF EXISTS public.prevent_manual_actual_hours_update();
DROP FUNCTION IF EXISTS public.get_random_team_member(p_employee_id integer, p_team_id integer);
DROP FUNCTION IF EXISTS public.generate_task_history(p_task_id integer);
DROP FUNCTION IF EXISTS public.add_work_hours(p_start timestamp without time zone, p_hours numeric, p_hours_per_day numeric);
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
-- Data for Name: employee; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.employee VALUES (1, 'Иванов', 'Иван', 'Сергеевич', 'Инженер-программист', 'Senior', 2, 8.0, false);
INSERT INTO public.employee VALUES (2, 'Петрова', 'Анна', 'Александровна', 'Инженер-программист', 'Middle', 2, 6.0, false);
INSERT INTO public.employee VALUES (3, 'Сидоров', 'Сергей', 'Викторович', 'Ведущий инженер-конструктор', 'Lead', 1, 8.0, false);
INSERT INTO public.employee VALUES (4, 'Козлов', 'Дмитрий', 'Игоревич', 'Инженер-программист', 'Junior', 2, 8.0, false);
INSERT INTO public.employee VALUES (5, 'Фёдорова', 'Елена', 'Владимировна', 'Инженер по тестированию', 'Middle', 3, 6.0, false);
INSERT INTO public.employee VALUES (6, 'Морозов', 'Андрей', 'Николаевич', 'Инженер DevOps', 'Senior', 2, 8.0, false);
INSERT INTO public.employee VALUES (7, 'Волкова', 'Мария', 'Петровна', 'Инженер-аналитик', 'Middle', 3, 8.0, false);
INSERT INTO public.employee VALUES (8, 'Новиков', 'Павел', 'Артёмович', 'Инженер-конструктор', 'Middle', 1, 6.0, false);
INSERT INTO public.employee VALUES (9, 'Смирнова', 'Ольга', 'Дмитриевна', 'Руководитель проекта', 'Senior', 1, 8.0, false);
INSERT INTO public.employee VALUES (10, 'Фрилансер', 'Алексей', NULL, 'Специалист по внедрению', 'Middle', NULL, 8.0, false);
INSERT INTO public.employee VALUES (11, 'Дмитров', 'Дмитрий', 'Сергеевич', 'Системный администратор', 'Middle', 1, 8.0, false);
INSERT INTO public.employee VALUES (12, 'Дмитров', 'Дмитрий', 'Сергеевич', 'Системный администратор', 'Middle', 1, 8.0, false);
INSERT INTO public.employee VALUES (13, 'авыфа', 'авфаыв', 'авфыа', 'авфы', 'Middle', 2, 8.0, true);


--
-- Data for Name: employee_project; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- Data for Name: priority; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.priority VALUES (1, 'Критичный', 100);
INSERT INTO public.priority VALUES (2, 'Высокий', 75);
INSERT INTO public.priority VALUES (3, 'Средний', 50);
INSERT INTO public.priority VALUES (4, 'Низкий', 25);


--
-- Data for Name: project; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.project VALUES (1, 'Автоматизированная система испытаний электродвигателей', 'Разработка ПО для автоматизации приёмо-сдаточных испытаний тяговых двигателей', 'active', 9, 2, NULL);
INSERT INTO public.project VALUES (2, 'Мониторинг состояния тягового оборудования', 'Система непрерывного контроля параметров оборудования в реальном времени', 'active', 9, 1, NULL);
INSERT INTO public.project VALUES (4, 'Старая система учёта', 'Унаследованная система, выведенная из эксплуатации', 'completed', 9, 1, '2026-04-20 18:00:00');
INSERT INTO public.project VALUES (3, 'Электронный архив технической документации', 'Перевод бумажного архива КБ в цифровой формат с поиском', 'planning', 7, 3, NULL);


--
-- Data for Name: sprint; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.sprint VALUES (1, 'Первая половина мая', '2026-05-01', '2026-05-15', 11, false);
INSERT INTO public.sprint VALUES (2, 'Вторая половина мая', '2026-05-16', '2026-05-31', 11, false);
INSERT INTO public.sprint VALUES (3, 'Первая половина июня', '2026-06-01', '2026-06-15', 10, false);
INSERT INTO public.sprint VALUES (4, 'Вторая половина июня', '2026-06-16', '2026-06-30', 11, true);


--
-- Data for Name: task; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.task VALUES (46, 'Настроить резервное копирование базы испытаний', NULL, 5, 5, 2, 5.0, 26.0, '2026-06-13', 1, 1, 3, 2, '2026-06-09 19:56:20.087649', '2026-06-21 11:14:39.451107');
INSERT INTO public.task VALUES (49, 'Согласовать методику приёмо-сдаточных испытаний', NULL, 4, 5, 2, 3.0, 25.0, '2026-06-12', 8, 1, 3, 2, '2026-06-09 20:00:35.006109', '2026-06-21 11:14:58.842847');
INSERT INTO public.task VALUES (62, 'Настроить CI/CD для нового репозитория', NULL, 5, 2, 3, 4.0, 0.0, '2026-06-19', 6, 1, 4, 1, '2026-06-21 12:09:29.828935', '2026-06-25 12:41:04.991903');
INSERT INTO public.task VALUES (42, 'Разработать модуль экспорта отчетов в Excel', NULL, 1, 5, 2, 12.0, 10.0, '2026-06-10', 1, 1, 3, 2, '2026-06-07 15:00:06.363425', '2026-06-21 11:19:58.063083');
INSERT INTO public.task VALUES (1, 'Разработать программу испытаний для двигателя ДТК-417', 'Алгоритм проверки сопротивления изоляции', 1, 5, 1, 16.0, 24.4, '2026-05-28', 4, 1, 2, 1, '2026-05-16 09:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (2, 'Создать интерфейс оператора стенда', 'Формы отображения параметров', 1, 5, 1, 20.0, 26.3, '2026-05-22', 1, 1, 2, 1, '2026-05-16 09:30:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (3, 'Написать модульные тесты для протокола', 'Покрытие ключевых функций', 2, 5, 3, 8.0, 11.0, '2026-05-30', 1, 1, 2, 1, '2026-05-17 10:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (35, 'Провести ревизию прав доступа', 'Аудит учётных записей', 4, 5, 3, 4.0, 5.1, '2026-04-30', 9, 4, NULL, 1, '2026-04-16 09:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (36, 'Разработать резервное копирование', 'Настройка бэкапов', 5, 5, 2, 10.0, 11.3, '2026-04-28', 1, 4, NULL, 1, '2026-04-18 10:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (37, 'Обновить сертификаты', 'Продление SSL', 5, 5, 4, 2.0, 3.0, '2026-04-15', 9, 4, NULL, 1, '2026-04-20 08:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (43, 'Обновить дизайн карточек задач в канбане', NULL, 3, 5, 3, 6.0, 4.0, '2026-06-12', 8, 2, 3, 2, '2026-06-09 19:53:14.016876', '2026-06-16 18:38:48.999328');
INSERT INTO public.task VALUES (34, 'Исправить баг с отображением графиков', 'Старый модуль', 1, 5, 1, 12.0, 10.2, '2026-04-18', 1, 4, NULL, 1, '2026-04-15 11:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (53, 'Написать модульные тесты для API', NULL, 2, 1, 3, 8.0, 0.0, '2026-06-24', 5, 1, 4, 1, '2026-06-21 11:33:11.607705', NULL);
INSERT INTO public.task VALUES (56, 'Исправить ошибку расчета мощности', NULL, 1, 2, 1, 4.0, 0.0, '2026-06-19', NULL, 1, 4, 1, '2026-06-21 11:42:24.031742', '2026-06-25 12:43:40.940152');
INSERT INTO public.task VALUES (39, 'Срочно исправить утечку соединений в пуле', 'Критический баг, соединения не возвращаются', 1, 6, 1, 6.0, 0.0, '2026-05-26', 1, 1, 2, 1, '2026-05-25 14:00:00', '2026-06-07 14:23:46.311838');
INSERT INTO public.task VALUES (40, 'Реализовать пагинацию для списка задач', 'Добавить постраничную навигацию', 1, 6, 3, 12.0, 0.0, '2026-06-05', 1, 1, 2, 1, '2026-05-24 09:00:00', '2026-06-07 14:23:46.311838');
INSERT INTO public.task VALUES (38, 'Подготовить скрипты миграции для тестовой базы', 'Написать скрипты на Flyway', 1, 6, 2, 4.0, 0.0, '2026-05-28', 1, 1, 2, 1, '2026-05-26 10:00:00', '2026-06-07 14:23:46.311838');
INSERT INTO public.task VALUES (7, 'Обновить библиотеки промышленных протоколов', 'Modbus TCP 2.0', 5, 6, 4, 6.0, 0.0, '2026-05-31', NULL, 1, 2, 1, '2026-05-18 09:00:00', '2026-06-07 14:23:46.311838');
INSERT INTO public.task VALUES (24, 'Исследовать возможность внедрения ИИ', 'Пилотный проект', 6, 6, 4, 10.0, 0.0, '2026-06-05', NULL, 3, 2, 1, '2026-05-25 11:00:00', '2026-06-07 14:23:46.311838');
INSERT INTO public.task VALUES (59, 'Разработать дешборд для руководства', NULL, 1, 5, 2, 20.0, 21.0, '2026-06-30', 1, 2, 4, 1, '2026-06-21 11:46:38.229885', '2026-06-25 12:48:25.325534');
INSERT INTO public.task VALUES (50, 'Разработать модуль интеграции с 1С:Предприятие', NULL, 1, 2, 1, 24.0, 0.0, '2026-06-27', 1, 1, 4, 1, '2026-06-21 11:28:24.80911', '2026-06-25 12:49:20.135364');
INSERT INTO public.task VALUES (64, 'Разработать ПО для тестирования нового двигателя', NULL, 1, 2, 3, 60.0, 0.0, '2026-06-30', 1, 1, 4, 1, '2026-06-25 12:57:20.788469', '2026-06-25 13:22:51.679357');
INSERT INTO public.task VALUES (44, 'Провести код-ревью модуля диагностики', 'fdasfsafsdafasdfasd1231236666661231123svaga', 2, 5, 1, 4.0, 2.0, '2026-06-14', 2, 1, 3, 2, '2026-06-09 19:54:24.447423', '2026-06-21 11:20:36.430171');
INSERT INTO public.task VALUES (41, 'Протестировать модуль интеграции с 1С', 'Проверить обмен данными после доработок', 2, 6, 2, 8.0, 7.0, '2026-05-29', 1, 1, 2, 1, '2026-05-23 08:00:00', '2026-06-07 14:23:46.311838');
INSERT INTO public.task VALUES (51, 'Реализовать выгрузку отчетов в PDF', NULL, 1, 4, 2, 16.0, 12.0, '2026-06-26', 2, 2, 4, 1, '2026-06-21 11:29:29.499198', '2026-06-25 12:54:00.925242');
INSERT INTO public.task VALUES (54, 'Обновить библиотеки до последних версий', NULL, 5, 1, 4, 4.0, 0.0, '2026-06-30', 6, 3, 4, 1, '2026-06-21 11:36:12.248224', NULL);
INSERT INTO public.task VALUES (47, 'Подготовить тестовые сценарии для нагрузочного тестирования', NULL, 2, 5, 3, 10.0, 8.0, '2026-06-11', 5, 2, 3, 2, '2026-06-09 19:57:30.53263', '2026-06-16 18:34:39.718624');
INSERT INTO public.task VALUES (57, 'Срочно обновить SSL-сертификаты', NULL, 5, 3, 1, 3.0, 28.4, '2026-06-20', 6, 1, 4, 1, '2026-06-21 11:43:36.649642', '2026-06-25 13:23:00.698408');
INSERT INTO public.task VALUES (4, 'Спроектировать структуру БД испытаний', 'ER-диаграмма, скрипты миграций', 4, 5, 2, 10.0, 12.0, '2026-05-20', 9, 1, 2, 1, '2026-05-17 11:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (5, 'Разработать мнемосхему стенда', 'Графическое отображение узлов', 3, 5, 2, 12.0, 13.1, '2026-05-17', 9, 1, 2, 1, '2026-05-17 12:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (6, 'Исправить ошибку расчёта коэффициента', 'Баг в формуле приведения', 1, 5, 1, 4.0, 5.1, '2026-05-24', 2, 1, 2, 1, '2026-05-18 08:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (8, 'Добавить валидацию входных сигналов АЦП', 'Защита от некорректных данных', 1, 5, 2, 8.0, 8.9, '2026-05-27', 4, 1, 2, 1, '2026-05-18 10:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (9, 'Настроить CI/CD для сборки образов', 'GitHub Actions', 5, 5, 3, 6.0, 8.0, '2026-06-01', 2, 1, 2, 1, '2026-05-19 08:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (10, 'Реализовать фоновую синхронизацию', 'Обмен данными между модулями', 1, 5, 2, 20.0, 25.2, '2026-05-29', 1, 2, 2, 1, '2026-05-19 09:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (11, 'Разработать API для мобильного приложения', 'REST API', 1, 5, 2, 25.0, 31.8, '2026-05-31', 6, 2, 2, 1, '2026-05-20 08:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (12, 'Подготовить 3D-модель узла крепления', 'SolidWorks', 3, 5, 2, 18.0, 24.0, '2026-05-29', 3, 2, 2, 1, '2026-05-20 09:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (14, 'Провести испытания на вибростенде', 'Проверка прототипа', 2, 5, 2, 10.0, 13.1, '2026-05-25', 7, 2, 2, 1, '2026-05-21 09:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (63, 'Оптимизировать запросы для отчетов по испытаниям', NULL, 2, 5, 1, 24.0, 22.0, '2026-06-29', 1, 1, 4, 1, '2026-06-21 12:11:59.937882', '2026-06-25 12:40:14.177164');
INSERT INTO public.task VALUES (60, 'Добавить логгирование в модуль диагностики', NULL, 1, 3, 3, 6.0, 5.0, '2026-06-30', 7, 1, 4, 1, '2026-06-21 11:52:59.079524', '2026-06-25 12:50:36.535786');
INSERT INTO public.task VALUES (48, 'Разработать прототип дашборда для руководства', NULL, 1, 5, 1, 16.0, 27.0, '2026-06-13', 4, 3, 3, 2, '2026-06-09 19:58:46.921799', '2026-06-21 11:15:14.736714');
INSERT INTO public.task VALUES (61, 'Составить план-график на июль', NULL, 4, 5, 4, 2.0, 3.0, '2026-06-29', 9, 3, 4, 1, '2026-06-21 11:56:35.044914', '2026-06-25 12:44:41.894974');
INSERT INTO public.task VALUES (45, 'Написать документацию по API для мобильного приложения', 'fdafsda', 4, 5, 4, 8.0, 7.0, '2026-06-15', 7, 2, 3, 2, '2026-06-09 19:55:25.299962', '2026-06-21 11:18:08.686387');
INSERT INTO public.task VALUES (52, 'Создать макет интерфейса мобильной версии', NULL, 3, 1, 3, 10.0, 0.0, '2026-06-25', 8, 2, 4, 1, '2026-06-21 11:30:28.527455', NULL);
INSERT INTO public.task VALUES (55, 'Подготовить презентацию для совещания', NULL, 4, 2, 2, 6.0, 0.0, '2026-06-23', 9, 3, 4, 1, '2026-06-21 11:40:08.7976', '2026-06-25 12:44:03.507467');
INSERT INTO public.task VALUES (15, 'Срочная: обновить прошивку датчиков', 'Критическое обновление', 5, 6, 1, 15.0, 16.1, '2026-05-25', 4, 1, 2, 1, '2026-05-22 08:00:00', '2026-06-07 14:23:46.311838');
INSERT INTO public.task VALUES (17, 'Согласовать методику испытаний', 'Утверждение документа', 4, 6, 2, 12.0, 12.3, '2026-05-28', 8, 1, 2, 1, '2026-05-23 08:00:00', '2026-06-07 14:23:46.311838');
INSERT INTO public.task VALUES (18, 'Провести анализ рынка комплектующих', 'Обзор поставщиков', 6, 6, 3, 8.0, 8.8, '2026-05-30', 5, 2, 2, 1, '2026-05-23 09:00:00', '2026-06-07 14:23:46.311838');
INSERT INTO public.task VALUES (20, 'Внеплановый аудит безопасности', 'Проверка после инцидента', 5, 6, 1, 10.0, 10.7, '2026-05-29', 7, 1, 2, 1, '2026-05-24 09:00:00', '2026-06-07 14:23:46.311838');
INSERT INTO public.task VALUES (21, 'Добавить логирование в модуль диагностики', 'По требованию заказчика', 1, 6, 2, 12.0, 11.2, '2026-05-31', 4, 1, 2, 1, '2026-05-25 08:00:00', '2026-06-07 14:23:46.311838');
INSERT INTO public.task VALUES (22, 'Разработать модуль интеграции с ERP', 'Обмен с 1С', 1, 6, 1, 40.0, 44.6, '2026-05-30', 2, 1, 2, 1, '2026-05-25 09:00:00', '2026-06-07 14:23:46.311838');
INSERT INTO public.task VALUES (23, 'Реализовать фоновую синхронизацию (доп.)', 'Доп. работы по синхронизации', 1, 6, 2, 25.0, 26.1, '2026-05-30', 6, 2, 2, 1, '2026-05-25 10:00:00', '2026-06-07 14:23:46.311838');
INSERT INTO public.task VALUES (58, 'Провести нагрузочное тестирование', NULL, 2, 5, 2, 8.0, 5.0, '2026-06-29', 5, 2, 4, 1, '2026-06-21 11:44:40.586338', '2026-06-25 12:52:50.75199');
INSERT INTO public.task VALUES (13, 'Реализовать модуль сбора данных с датчиков', 'Опрос акселерометров', 1, 5, 1, 14.0, 16.4, '2026-06-02', 2, 2, 2, 1, '2026-05-21 08:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (16, 'Подготовить отчёт по нагрузочному тестированию', 'Для руководства', 4, 5, 3, 8.0, 9.7, '2026-05-28', 7, 2, 2, 1, '2026-05-22 09:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (19, 'Подготовить презентацию для совещания', 'Слайды по статусу проекта', 6, 5, 4, 6.0, 6.8, '2026-05-29', 8, 1, 2, 1, '2026-05-24 08:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (25, 'Утвердить техническое задание', 'Согласование ТЗ', 4, 5, 2, 8.0, 10.1, '2026-05-10', 9, 1, 1, 1, '2026-05-01 09:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (26, 'Разработать прототип интерфейса', 'Figma', 3, 5, 3, 12.0, 15.3, '2026-05-12', 9, 2, 1, 1, '2026-05-02 10:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (27, 'Настроить тестовый стенд', 'Установка оборудования', 5, 5, 2, 10.0, 13.5, '2026-05-14', 1, 1, 1, 1, '2026-05-03 08:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (28, 'Провести нагрузочное тестирование', 'Под высокой нагрузкой', 2, 5, 1, 6.0, 6.9, '2026-05-13', 7, 2, 1, 1, '2026-05-04 09:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (29, 'Составить план-график работ', 'Диаграмма Ганта', 4, 5, 4, 4.0, 5.6, '2026-05-08', 5, 3, 1, 1, '2026-05-05 11:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (30, 'Согласовать изменения в ТЗ', 'Правки по требованию', 4, 5, 2, 6.0, 8.1, '2026-05-14', 8, 3, 1, 1, '2026-05-06 10:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (31, 'Перенести данные из MS Access', 'Миграция архивов', 5, 5, 2, 20.0, 26.3, '2026-04-20', 1, 4, NULL, 1, '2026-04-10 09:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (32, 'Обновить серверное ПО', 'Установка обновлений', 5, 5, 3, 8.0, 10.1, '2026-04-22', 2, 4, NULL, 1, '2026-04-12 10:00:00', '2026-05-26 01:12:10.047902');
INSERT INTO public.task VALUES (33, 'Написать инструкцию администратора', 'Документация', 4, 5, 4, 6.0, 7.7, '2026-04-25', 5, 4, NULL, 1, '2026-04-14 08:00:00', '2026-05-26 01:12:10.047902');


--
-- Data for Name: task_history; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.task_history VALUES (1, 1, NULL, 1, 1, '2026-05-16 09:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (2, 1, 1, 2, 1, '2026-05-16 12:46:38.427673', 1, 0.0);
INSERT INTO public.task_history VALUES (3, 1, 2, 3, 1, '2026-05-20 12:01:32.388956', 6, 19.0);
INSERT INTO public.task_history VALUES (4, 1, 3, 4, 1, '2026-05-20 14:32:40.782809', 4, 2.5);
INSERT INTO public.task_history VALUES (5, 1, 4, 5, 1, '2026-05-21 09:27:19.849533', 4, 2.9);
INSERT INTO public.task_history VALUES (6, 2, NULL, 1, 1, '2026-05-16 09:30:00', 2, 0.0);
INSERT INTO public.task_history VALUES (7, 2, 1, 2, 1, '2026-05-16 12:40:17.152166', 2, 0.0);
INSERT INTO public.task_history VALUES (8, 2, 2, 3, 1, '2026-05-21 10:14:02.913635', 6, 19.2);
INSERT INTO public.task_history VALUES (9, 2, 3, 4, 1, '2026-05-21 13:36:46.856025', 1, 3.4);
INSERT INTO public.task_history VALUES (10, 2, 4, 5, 1, '2026-05-22 11:18:15.124492', 1, 3.7);
INSERT INTO public.task_history VALUES (11, 3, NULL, 1, 1, '2026-05-17 10:00:00', 4, 0.0);
INSERT INTO public.task_history VALUES (12, 3, 1, 2, 1, '2026-05-17 13:15:19.668092', 4, 0.0);
INSERT INTO public.task_history VALUES (13, 3, 2, 3, 1, '2026-05-19 10:03:38.874865', 6, 9.1);
INSERT INTO public.task_history VALUES (14, 3, 3, 4, 1, '2026-05-19 10:33:58.953482', 1, 0.5);
INSERT INTO public.task_history VALUES (15, 3, 4, 5, 1, '2026-05-19 11:57:31.093511', 1, 1.4);
INSERT INTO public.task_history VALUES (16, 4, NULL, 1, 1, '2026-05-17 11:00:00', 3, 0.0);
INSERT INTO public.task_history VALUES (17, 4, 1, 2, 1, '2026-05-17 14:20:04.736553', 3, 0.0);
INSERT INTO public.task_history VALUES (18, 4, 2, 3, 1, '2026-05-19 10:25:08.918623', 8, 9.4);
INSERT INTO public.task_history VALUES (19, 4, 3, 4, 1, '2026-05-19 11:46:20.536412', 9, 1.4);
INSERT INTO public.task_history VALUES (20, 4, 4, 5, 1, '2026-05-19 12:56:34.957136', 9, 1.2);
INSERT INTO public.task_history VALUES (21, 5, NULL, 1, 1, '2026-05-17 12:00:00', 8, 0.0);
INSERT INTO public.task_history VALUES (22, 5, 1, 2, 1, '2026-05-17 12:16:07.849904', 8, 0.0);
INSERT INTO public.task_history VALUES (23, 5, 2, 3, 1, '2026-05-19 13:52:31.48758', 3, 10.9);
INSERT INTO public.task_history VALUES (24, 5, 3, 4, 1, '2026-05-19 14:43:32.505628', 9, 0.9);
INSERT INTO public.task_history VALUES (25, 5, 4, 5, 1, '2026-05-20 10:00:15.481767', 9, 1.3);
INSERT INTO public.task_history VALUES (26, 6, NULL, 1, 1, '2026-05-18 08:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (27, 6, 1, 2, 1, '2026-05-18 08:22:33.062813', 1, 0.0);
INSERT INTO public.task_history VALUES (28, 6, 2, 3, 1, '2026-05-18 12:58:25.026579', 2, 4.0);
INSERT INTO public.task_history VALUES (29, 6, 3, 4, 1, '2026-05-18 13:32:50.287288', 2, 0.6);
INSERT INTO public.task_history VALUES (30, 6, 4, 5, 1, '2026-05-18 14:04:38.612431', 2, 0.5);
INSERT INTO public.task_history VALUES (31, 8, NULL, 1, 1, '2026-05-18 10:00:00', 2, 0.0);
INSERT INTO public.task_history VALUES (32, 8, 1, 2, 1, '2026-05-18 10:09:07.988551', 2, 0.0);
INSERT INTO public.task_history VALUES (33, 8, 2, 3, 1, '2026-05-19 11:23:12.888357', 4, 7.2);
INSERT INTO public.task_history VALUES (34, 8, 3, 4, 1, '2026-05-19 12:04:15.779265', 4, 0.7);
INSERT INTO public.task_history VALUES (35, 8, 4, 5, 1, '2026-05-19 13:05:58.590291', 4, 1.0);
INSERT INTO public.task_history VALUES (36, 9, NULL, 1, 1, '2026-05-19 08:00:00', 6, 0.0);
INSERT INTO public.task_history VALUES (37, 9, 1, 2, 1, '2026-05-19 10:07:54.174272', 6, 0.0);
INSERT INTO public.task_history VALUES (38, 9, 2, 3, 1, '2026-05-19 16:35:26.121149', 4, 6.5);
INSERT INTO public.task_history VALUES (39, 9, 3, 4, 1, '2026-05-20 09:04:38.951404', 2, 0.5);
INSERT INTO public.task_history VALUES (40, 9, 4, 5, 1, '2026-05-20 10:06:33.495704', 2, 1.0);
INSERT INTO public.task_history VALUES (41, 10, NULL, 1, 1, '2026-05-19 09:00:00', 4, 0.0);
INSERT INTO public.task_history VALUES (42, 10, 1, 2, 1, '2026-05-19 10:54:20.336422', 4, 0.0);
INSERT INTO public.task_history VALUES (43, 10, 2, 3, 1, '2026-05-21 15:00:34.742312', 2, 20.1);
INSERT INTO public.task_history VALUES (44, 10, 3, 4, 1, '2026-05-22 10:17:16.283856', 1, 3.3);
INSERT INTO public.task_history VALUES (45, 10, 4, 5, 1, '2026-05-22 12:04:29.55181', 1, 1.8);
INSERT INTO public.task_history VALUES (46, 11, NULL, 1, 1, '2026-05-20 08:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (47, 11, 1, 2, 1, '2026-05-20 08:11:11.704023', 1, 0.0);
INSERT INTO public.task_history VALUES (48, 11, 2, 3, 1, '2026-05-25 10:42:14.007663', 6, 25.7);
INSERT INTO public.task_history VALUES (49, 11, 3, 4, 1, '2026-05-25 14:42:14.007663', 6, 4.0);
INSERT INTO public.task_history VALUES (50, 11, 4, 5, 1, '2026-05-25 16:50:45.824014', 6, 2.1);
INSERT INTO public.task_history VALUES (51, 12, NULL, 1, 1, '2026-05-20 09:00:00', 8, 0.0);
INSERT INTO public.task_history VALUES (52, 12, 1, 2, 1, '2026-05-20 11:05:04.25365', 8, 0.0);
INSERT INTO public.task_history VALUES (53, 12, 2, 3, 1, '2026-05-25 11:26:38.128896', 3, 18.4);
INSERT INTO public.task_history VALUES (54, 12, 3, 4, 1, '2026-05-25 14:51:33.375933', 3, 3.4);
INSERT INTO public.task_history VALUES (55, 12, 4, 5, 1, '2026-05-26 11:01:51.638052', 3, 2.2);
INSERT INTO public.task_history VALUES (56, 13, NULL, 1, 1, '2026-05-21 08:00:00', 4, 0.0);
INSERT INTO public.task_history VALUES (57, 13, 1, 2, 1, '2026-05-21 10:44:35.9558', 4, 0.0);
INSERT INTO public.task_history VALUES (58, 13, 2, 3, 1, '2026-05-22 16:09:50.051509', 1, 13.4);
INSERT INTO public.task_history VALUES (59, 13, 3, 4, 1, '2026-05-25 09:44:10.086872', 2, 1.6);
INSERT INTO public.task_history VALUES (60, 13, 4, 5, 1, '2026-05-25 11:10:34.372513', 2, 1.4);
INSERT INTO public.task_history VALUES (61, 14, NULL, 1, 1, '2026-05-21 09:00:00', 5, 0.0);
INSERT INTO public.task_history VALUES (62, 14, 1, 2, 1, '2026-05-21 12:47:46.967501', 5, 0.0);
INSERT INTO public.task_history VALUES (63, 14, 2, 3, 1, '2026-05-25 11:14:55.313965', 7, 10.5);
INSERT INTO public.task_history VALUES (64, 14, 3, 4, 1, '2026-05-25 12:05:41.472139', 7, 0.8);
INSERT INTO public.task_history VALUES (65, 14, 4, 5, 1, '2026-05-25 13:52:21.11539', 7, 1.8);
INSERT INTO public.task_history VALUES (66, 15, NULL, 1, 1, '2026-05-22 08:00:00', 6, 0.0);
INSERT INTO public.task_history VALUES (67, 15, 1, 2, 1, '2026-05-22 09:37:03.330555', 6, 0.0);
INSERT INTO public.task_history VALUES (68, 15, 2, 3, 1, '2026-05-26 09:42:52.977835', 4, 16.1);
INSERT INTO public.task_history VALUES (69, 16, NULL, 1, 1, '2026-05-22 09:00:00', 5, 0.0);
INSERT INTO public.task_history VALUES (70, 16, 1, 2, 1, '2026-05-22 10:52:15.556788', 5, 0.0);
INSERT INTO public.task_history VALUES (71, 16, 2, 3, 1, '2026-05-25 12:24:38.180898', 7, 7.5);
INSERT INTO public.task_history VALUES (72, 16, 3, 4, 1, '2026-05-25 13:05:27.903274', 7, 0.7);
INSERT INTO public.task_history VALUES (73, 16, 4, 5, 1, '2026-05-25 14:36:13.930052', 7, 1.5);
INSERT INTO public.task_history VALUES (74, 17, NULL, 1, 1, '2026-05-23 08:00:00', 3, 0.0);
INSERT INTO public.task_history VALUES (75, 17, 1, 2, 1, '2026-05-23 09:56:21.746787', 3, 0.0);
INSERT INTO public.task_history VALUES (76, 17, 2, 3, 1, '2026-05-26 13:17:32.713326', 8, 12.3);
INSERT INTO public.task_history VALUES (77, 18, NULL, 1, 1, '2026-05-23 09:00:00', 7, 0.0);
INSERT INTO public.task_history VALUES (78, 18, 1, 2, 1, '2026-05-23 12:12:44.55223', 7, 0.0);
INSERT INTO public.task_history VALUES (79, 18, 2, 3, 1, '2026-05-26 09:46:13.437715', 5, 8.8);
INSERT INTO public.task_history VALUES (80, 19, NULL, 1, 1, '2026-05-24 08:00:00', 9, 0.0);
INSERT INTO public.task_history VALUES (81, 19, 1, 2, 1, '2026-05-24 09:24:47.245921', 9, 0.0);
INSERT INTO public.task_history VALUES (82, 19, 2, 3, 1, '2026-05-25 14:27:55.593191', 8, 5.5);
INSERT INTO public.task_history VALUES (83, 19, 3, 4, 1, '2026-05-25 14:50:19.210674', 8, 0.4);
INSERT INTO public.task_history VALUES (84, 19, 4, 5, 1, '2026-05-25 15:43:25.514947', 8, 0.9);
INSERT INTO public.task_history VALUES (85, 20, NULL, 1, 1, '2026-05-24 09:00:00', 5, 0.0);
INSERT INTO public.task_history VALUES (86, 20, 1, 2, 1, '2026-05-24 10:54:45.320431', 5, 0.0);
INSERT INTO public.task_history VALUES (87, 20, 2, 3, 1, '2026-05-26 13:40:11.066161', 7, 10.7);
INSERT INTO public.task_history VALUES (88, 21, NULL, 1, 1, '2026-05-25 08:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (89, 21, 1, 2, 1, '2026-05-25 08:04:12.297831', 1, 0.0);
INSERT INTO public.task_history VALUES (90, 21, 2, 3, 1, '2026-05-26 12:09:19.634879', 4, 11.2);
INSERT INTO public.task_history VALUES (91, 22, NULL, 1, 1, '2026-05-25 09:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (92, 22, 1, 2, 1, '2026-05-25 12:33:28.968657', 1, 0.0);
INSERT INTO public.task_history VALUES (93, 22, 2, 3, 1, '2026-06-02 09:09:11.160863', 2, 44.6);
INSERT INTO public.task_history VALUES (94, 23, NULL, 1, 1, '2026-05-25 10:00:00', 4, 0.0);
INSERT INTO public.task_history VALUES (95, 23, 1, 2, 1, '2026-05-25 10:34:02.02336', 4, 0.0);
INSERT INTO public.task_history VALUES (96, 23, 2, 3, 1, '2026-05-28 12:42:25.082937', 6, 26.1);
INSERT INTO public.task_history VALUES (97, 25, NULL, 1, 1, '2026-05-01 09:00:00', 3, 0.0);
INSERT INTO public.task_history VALUES (98, 25, 1, 2, 1, '2026-05-03 09:53:11.790368', 3, 0.0);
INSERT INTO public.task_history VALUES (99, 25, 2, 3, 1, '2026-05-04 16:47:33.01902', 9, 7.8);
INSERT INTO public.task_history VALUES (100, 25, 3, 4, 1, '2026-05-05 10:02:34.518954', 9, 1.3);
INSERT INTO public.task_history VALUES (101, 25, 4, 5, 1, '2026-05-05 10:59:34.921964', 9, 1.0);
INSERT INTO public.task_history VALUES (102, 26, NULL, 1, 1, '2026-05-02 10:00:00', 8, 0.0);
INSERT INTO public.task_history VALUES (103, 26, 1, 2, 1, '2026-05-02 22:37:08.573513', 8, 0.0);
INSERT INTO public.task_history VALUES (104, 26, 2, 3, 1, '2026-05-06 09:18:00.288147', 3, 12.3);
INSERT INTO public.task_history VALUES (105, 26, 3, 4, 1, '2026-05-06 10:06:28.4688', 9, 0.8);
INSERT INTO public.task_history VALUES (106, 26, 4, 5, 1, '2026-05-06 12:19:11.529944', 9, 2.2);
INSERT INTO public.task_history VALUES (107, 27, NULL, 1, 1, '2026-05-03 08:00:00', 6, 0.0);
INSERT INTO public.task_history VALUES (108, 27, 1, 2, 1, '2026-05-03 08:04:49.966375', 6, 0.0);
INSERT INTO public.task_history VALUES (109, 27, 2, 3, 1, '2026-05-05 11:37:51.653778', 1, 10.6);
INSERT INTO public.task_history VALUES (110, 27, 3, 4, 1, '2026-05-05 12:57:11.206151', 1, 1.3);
INSERT INTO public.task_history VALUES (111, 27, 4, 5, 1, '2026-05-05 14:30:26.045817', 1, 1.6);
INSERT INTO public.task_history VALUES (112, 28, NULL, 1, 1, '2026-05-04 09:00:00', 5, 0.0);
INSERT INTO public.task_history VALUES (113, 28, 1, 2, 1, '2026-05-04 10:12:50.687163', 5, 0.0);
INSERT INTO public.task_history VALUES (114, 28, 2, 3, 1, '2026-05-05 09:55:16.145255', 7, 5.7);
INSERT INTO public.task_history VALUES (115, 28, 3, 4, 1, '2026-05-05 10:26:54.530172', 7, 0.5);
INSERT INTO public.task_history VALUES (116, 28, 4, 5, 1, '2026-05-05 11:08:27.849395', 7, 0.7);
INSERT INTO public.task_history VALUES (117, 29, NULL, 1, 1, '2026-05-05 11:00:00', 7, 0.0);
INSERT INTO public.task_history VALUES (118, 29, 1, 2, 1, '2026-05-05 14:59:57.692065', 7, 0.0);
INSERT INTO public.task_history VALUES (119, 29, 2, 3, 1, '2026-05-06 11:28:00.888313', 5, 4.5);
INSERT INTO public.task_history VALUES (120, 29, 3, 4, 1, '2026-05-06 12:01:44.661752', 5, 0.6);
INSERT INTO public.task_history VALUES (121, 29, 4, 5, 1, '2026-05-06 12:31:44.661752', 5, 0.5);
INSERT INTO public.task_history VALUES (122, 30, NULL, 1, 1, '2026-05-06 10:00:00', 9, 0.0);
INSERT INTO public.task_history VALUES (123, 30, 1, 2, 1, '2026-05-06 13:57:10.830801', 9, 0.0);
INSERT INTO public.task_history VALUES (124, 30, 2, 3, 1, '2026-05-07 12:31:49.087238', 8, 6.6);
INSERT INTO public.task_history VALUES (125, 30, 3, 4, 1, '2026-05-07 13:32:29.079373', 8, 1.0);
INSERT INTO public.task_history VALUES (126, 30, 4, 5, 1, '2026-05-07 14:02:29.079373', 8, 0.5);
INSERT INTO public.task_history VALUES (127, 31, NULL, 1, 1, '2026-04-10 09:00:00', 6, 0.0);
INSERT INTO public.task_history VALUES (128, 31, 1, 2, 1, '2026-04-10 09:25:28.41066', 6, 0.0);
INSERT INTO public.task_history VALUES (129, 31, 2, 3, 1, '2026-04-14 14:16:20.52354', 4, 20.8);
INSERT INTO public.task_history VALUES (130, 31, 3, 4, 1, '2026-04-14 16:41:02.561671', 1, 2.4);
INSERT INTO public.task_history VALUES (131, 31, 4, 5, 1, '2026-04-15 11:48:16.195191', 1, 3.1);
INSERT INTO public.task_history VALUES (132, 32, NULL, 1, 1, '2026-04-12 10:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (133, 32, 1, 2, 1, '2026-04-12 10:15:22.732105', 1, 0.0);
INSERT INTO public.task_history VALUES (134, 32, 2, 3, 1, '2026-04-13 16:20:45.044428', 2, 7.3);
INSERT INTO public.task_history VALUES (135, 32, 3, 4, 1, '2026-04-14 09:37:42.423435', 2, 1.3);
INSERT INTO public.task_history VALUES (136, 32, 4, 5, 1, '2026-04-14 11:10:15.963191', 2, 1.5);
INSERT INTO public.task_history VALUES (137, 33, NULL, 1, 1, '2026-04-14 08:00:00', 7, 0.0);
INSERT INTO public.task_history VALUES (138, 33, 1, 2, 1, '2026-04-14 08:52:05.930587', 7, 0.0);
INSERT INTO public.task_history VALUES (139, 33, 2, 3, 1, '2026-04-14 15:16:54.364071', 5, 6.3);
INSERT INTO public.task_history VALUES (140, 33, 3, 4, 1, '2026-04-14 15:46:29.668342', 5, 0.5);
INSERT INTO public.task_history VALUES (141, 33, 4, 5, 1, '2026-04-14 16:43:10.157393', 5, 0.9);
INSERT INTO public.task_history VALUES (142, 34, NULL, 1, 1, '2026-04-15 11:00:00', 2, 0.0);
INSERT INTO public.task_history VALUES (143, 34, 1, 2, 1, '2026-04-15 11:42:52.746691', 2, 0.0);
INSERT INTO public.task_history VALUES (145, 34, 3, 4, 1, '2026-04-17 14:24:13.917539', 1, 1.1);
INSERT INTO public.task_history VALUES (146, 34, 4, 5, 1, '2026-04-20 09:31:36.171216', 1, 1.1);
INSERT INTO public.task_history VALUES (147, 35, NULL, 1, 1, '2026-04-16 09:00:00', 3, 0.0);
INSERT INTO public.task_history VALUES (148, 35, 1, 2, 1, '2026-04-16 11:55:04.240425', 3, 0.0);
INSERT INTO public.task_history VALUES (149, 35, 2, 3, 1, '2026-04-16 15:56:16.030975', 9, 4.0);
INSERT INTO public.task_history VALUES (150, 35, 3, 4, 1, '2026-04-16 16:29:02.182579', 9, 0.5);
INSERT INTO public.task_history VALUES (151, 35, 4, 5, 1, '2026-04-17 09:04:28.157622', 9, 0.6);
INSERT INTO public.task_history VALUES (152, 36, NULL, 1, 1, '2026-04-18 10:00:00', 4, 0.0);
INSERT INTO public.task_history VALUES (153, 36, 1, 2, 1, '2026-04-18 10:08:24.397276', 4, 0.0);
INSERT INTO public.task_history VALUES (154, 36, 2, 3, 1, '2026-04-21 10:15:10.345509', 2, 9.3);
INSERT INTO public.task_history VALUES (155, 36, 3, 4, 1, '2026-04-21 10:58:17.931791', 1, 0.7);
INSERT INTO public.task_history VALUES (156, 36, 4, 5, 1, '2026-04-21 12:14:45.365657', 1, 1.3);
INSERT INTO public.task_history VALUES (157, 37, NULL, 1, 1, '2026-04-20 08:00:00', 8, 0.0);
INSERT INTO public.task_history VALUES (158, 37, 1, 2, 1, '2026-04-20 08:08:42.711831', 8, 0.0);
INSERT INTO public.task_history VALUES (159, 37, 2, 3, 1, '2026-04-20 11:11:21.285241', 3, 2.2);
INSERT INTO public.task_history VALUES (160, 37, 3, 4, 1, '2026-04-20 11:31:46.602217', 9, 0.3);
INSERT INTO public.task_history VALUES (161, 37, 4, 5, 1, '2026-04-20 12:01:46.602217', 9, 0.5);
INSERT INTO public.task_history VALUES (162, 38, NULL, 1, 1, '2026-05-26 10:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (163, 39, NULL, 1, 1, '2026-05-25 14:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (164, 39, 1, 2, 1, '2026-05-25 14:30:00', 1, 0.0);
INSERT INTO public.task_history VALUES (165, 40, NULL, 1, 1, '2026-05-24 09:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (166, 40, 1, 2, 1, '2026-05-24 11:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (167, 41, NULL, 1, 1, '2026-05-23 08:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (168, 41, 1, 2, 1, '2026-05-23 09:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (169, 41, 2, 3, 1, '2026-05-23 15:00:00', 1, 6.0);
INSERT INTO public.task_history VALUES (170, 41, 3, 4, 1, '2026-05-23 16:00:00', 3, 1.0);
INSERT INTO public.task_history VALUES (171, 41, 4, 4, 1, '2026-05-24 08:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (172, 39, 2, 6, 2, '2026-06-07 14:23:46.325228', 1, 0.0);
INSERT INTO public.task_history VALUES (173, 40, 2, 6, 2, '2026-06-07 14:23:46.367517', 1, 0.0);
INSERT INTO public.task_history VALUES (174, 38, 1, 6, 2, '2026-06-07 14:23:46.368308', 1, 0.0);
INSERT INTO public.task_history VALUES (175, 7, 1, 6, 2, '2026-06-07 14:23:46.369288', NULL, 0.0);
INSERT INTO public.task_history VALUES (176, 24, 1, 6, 2, '2026-06-07 14:23:46.369973', NULL, 0.0);
INSERT INTO public.task_history VALUES (177, 41, 4, 6, 2, '2026-06-07 14:23:46.370574', 1, 0.0);
INSERT INTO public.task_history VALUES (178, 15, 3, 6, 2, '2026-06-07 14:23:46.371208', 4, 0.0);
INSERT INTO public.task_history VALUES (179, 17, 3, 6, 2, '2026-06-07 14:23:46.371842', 8, 0.0);
INSERT INTO public.task_history VALUES (180, 18, 3, 6, 2, '2026-06-07 14:23:46.372669', 5, 0.0);
INSERT INTO public.task_history VALUES (181, 20, 3, 6, 2, '2026-06-07 14:23:46.37355', 7, 0.0);
INSERT INTO public.task_history VALUES (182, 21, 3, 6, 2, '2026-06-07 14:23:46.374237', 4, 0.0);
INSERT INTO public.task_history VALUES (183, 22, 3, 6, 2, '2026-06-07 14:23:46.374882', 2, 0.0);
INSERT INTO public.task_history VALUES (184, 23, 3, 6, 2, '2026-06-07 14:23:46.375501', 6, 0.0);
INSERT INTO public.task_history VALUES (185, 42, NULL, 1, 2, '2026-06-07 15:00:06.383207', NULL, 0.0);
INSERT INTO public.task_history VALUES (186, 43, NULL, 1, 2, '2026-06-09 19:53:14.04657', 8, 0.0);
INSERT INTO public.task_history VALUES (187, 44, NULL, 1, 2, '2026-06-09 19:54:24.461878', 2, 0.0);
INSERT INTO public.task_history VALUES (188, 45, NULL, 1, 2, '2026-06-09 19:55:25.306813', 7, 0.0);
INSERT INTO public.task_history VALUES (189, 46, NULL, 1, 2, '2026-06-09 19:56:20.094675', 6, 0.0);
INSERT INTO public.task_history VALUES (190, 47, NULL, 1, 2, '2026-06-09 19:57:30.539372', 5, 0.0);
INSERT INTO public.task_history VALUES (191, 48, NULL, 1, 2, '2026-06-09 19:58:46.928951', 4, 0.0);
INSERT INTO public.task_history VALUES (192, 49, NULL, 1, 2, '2026-06-09 20:00:35.013223', 9, 0.0);
INSERT INTO public.task_history VALUES (193, 48, 1, 2, 2, '2026-06-10 21:36:14.360211', 4, NULL);
INSERT INTO public.task_history VALUES (194, 48, 2, 3, 2, '2026-06-15 21:26:59.406833', 4, 24.0);
INSERT INTO public.task_history VALUES (195, 49, 1, 2, 2, '2026-06-15 21:27:01.572043', 9, NULL);
INSERT INTO public.task_history VALUES (196, 47, 1, 2, 2, '2026-06-15 21:27:02.597357', 5, NULL);
INSERT INTO public.task_history VALUES (197, 46, 1, 2, 2, '2026-06-15 21:27:05.153802', 6, NULL);
INSERT INTO public.task_history VALUES (198, 47, 2, 3, 1, '2026-06-10 00:00:00', 5, 8.0);
INSERT INTO public.task_history VALUES (199, 47, 3, 5, 1, '2026-06-12 00:00:00', 5, 0.0);
INSERT INTO public.task_history VALUES (200, 43, 1, 2, 1, '2026-06-09 00:00:00', 8, 0.0);
INSERT INTO public.task_history VALUES (201, 43, 2, 3, 1, '2026-06-10 00:00:00', 8, 3.0);
INSERT INTO public.task_history VALUES (202, 43, 3, 5, 1, '2026-06-09 00:00:00', 8, 1.0);
INSERT INTO public.task_history VALUES (144, 34, 2, 3, 1, '2026-04-17 13:16:30.538994', 6, 8.0);
INSERT INTO public.task_history VALUES (203, 49, 2, 3, 2, '2026-06-18 23:24:12.272019', 9, 24.0);
INSERT INTO public.task_history VALUES (204, 46, 2, 3, 1, '2026-06-18 23:26:31.698554', 6, 24.0);
INSERT INTO public.task_history VALUES (205, 46, 3, 5, 1, '2026-06-19 00:00:00', 1, 2.0);
INSERT INTO public.task_history VALUES (206, 49, 3, 5, 1, '2026-06-19 00:00:00', 8, 1.0);
INSERT INTO public.task_history VALUES (207, 48, 3, 5, 1, '2026-06-16 00:00:00', 4, 3.0);
INSERT INTO public.task_history VALUES (208, 42, 1, 2, 1, '2026-06-08 00:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (209, 44, 1, 2, 1, '2026-06-11 00:00:00', 2, 0.0);
INSERT INTO public.task_history VALUES (210, 45, 1, 2, 1, '2026-06-10 00:00:00', 7, 0.0);
INSERT INTO public.task_history VALUES (211, 45, 2, 5, 1, '2026-06-11 00:00:00', 7, 7.0);
INSERT INTO public.task_history VALUES (212, 42, 2, 3, 1, '2026-06-10 00:00:00', 1, 8.0);
INSERT INTO public.task_history VALUES (213, 42, 3, 5, 1, '2026-06-10 00:00:00', 1, 2.0);
INSERT INTO public.task_history VALUES (214, 44, 2, 5, 1, '2026-06-10 00:00:00', 2, 2.0);
INSERT INTO public.task_history VALUES (215, 50, NULL, 1, 1, '2026-06-21 11:28:24.824845', 1, 0.0);
INSERT INTO public.task_history VALUES (216, 51, NULL, 1, 1, '2026-06-21 11:29:29.505973', 2, 0.0);
INSERT INTO public.task_history VALUES (217, 52, NULL, 1, 1, '2026-06-21 11:30:28.534035', 8, 0.0);
INSERT INTO public.task_history VALUES (218, 53, NULL, 1, 1, '2026-06-21 11:33:11.614582', 5, 0.0);
INSERT INTO public.task_history VALUES (219, 54, NULL, 1, 1, '2026-06-21 11:36:12.254822', 6, 0.0);
INSERT INTO public.task_history VALUES (220, 55, NULL, 1, 1, '2026-06-21 11:40:08.804597', 9, 0.0);
INSERT INTO public.task_history VALUES (221, 56, NULL, 1, 1, '2026-06-21 11:42:24.043341', NULL, 0.0);
INSERT INTO public.task_history VALUES (222, 57, NULL, 1, 1, '2026-06-21 11:43:36.656107', 6, 0.0);
INSERT INTO public.task_history VALUES (223, 58, NULL, 1, 1, '2026-06-21 11:44:40.59365', 5, 0.0);
INSERT INTO public.task_history VALUES (224, 59, NULL, 1, 1, '2026-06-21 11:46:38.236237', 1, 0.0);
INSERT INTO public.task_history VALUES (225, 60, NULL, 1, 1, '2026-06-21 11:52:59.085878', 7, 0.0);
INSERT INTO public.task_history VALUES (226, 61, NULL, 1, 1, '2026-06-21 11:56:35.051917', 9, 0.0);
INSERT INTO public.task_history VALUES (227, 62, NULL, 1, 1, '2026-06-21 12:09:29.835587', 6, 0.0);
INSERT INTO public.task_history VALUES (228, 63, NULL, 1, 1, '2026-06-21 12:11:59.945965', 1, 0.0);
INSERT INTO public.task_history VALUES (229, 63, 1, 2, 1, '2026-06-22 00:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (230, 63, 2, 3, 1, '2026-06-24 00:00:00', 1, 19.0);
INSERT INTO public.task_history VALUES (231, 63, 3, 5, 1, '2026-06-25 00:00:00', 1, 3.0);
INSERT INTO public.task_history VALUES (232, 62, 1, 2, 1, '2026-06-22 00:00:00', 6, 0.0);
INSERT INTO public.task_history VALUES (233, 61, 1, 2, 1, '2026-06-23 00:00:00', 9, 0.0);
INSERT INTO public.task_history VALUES (234, 57, 1, 2, 1, '2026-06-22 00:00:00', 6, 0.0);
INSERT INTO public.task_history VALUES (235, 56, 1, 2, 1, '2026-06-23 00:00:00', NULL, 0.0);
INSERT INTO public.task_history VALUES (236, 55, 1, 2, 1, '2026-06-21 00:00:00', 9, 0.0);
INSERT INTO public.task_history VALUES (237, 61, 2, 5, 1, '2026-06-25 00:00:00', 9, 3.0);
INSERT INTO public.task_history VALUES (238, 59, 1, 2, 1, '2026-06-25 12:46:24.990461', 1, NULL);
INSERT INTO public.task_history VALUES (239, 59, 2, 3, 1, '2026-06-23 00:00:00', 1, 18.0);
INSERT INTO public.task_history VALUES (240, 59, 3, 5, 1, '2026-06-24 00:00:00', 1, 3.0);
INSERT INTO public.task_history VALUES (241, 50, 1, 2, 1, '2026-06-24 00:00:00', 1, 0.0);
INSERT INTO public.task_history VALUES (242, 60, 1, 2, 1, '2026-06-22 00:00:00', 7, 0.0);
INSERT INTO public.task_history VALUES (243, 60, 2, 3, 1, '2026-06-23 00:00:00', 7, 5.0);
INSERT INTO public.task_history VALUES (244, 58, 1, 2, 1, '2026-06-22 00:00:00', 5, 0.0);
INSERT INTO public.task_history VALUES (245, 58, 2, 5, 1, '2026-06-23 00:00:00', 5, 5.0);
INSERT INTO public.task_history VALUES (246, 51, 1, 2, 1, '2026-06-23 00:00:00', 2, 0.0);
INSERT INTO public.task_history VALUES (247, 51, 2, 4, 1, '2026-06-24 00:00:00', 2, 12.0);
INSERT INTO public.task_history VALUES (248, 64, NULL, 1, 1, '2026-06-25 12:57:20.804509', 1, 0.0);
INSERT INTO public.task_history VALUES (249, 64, 1, 2, 2, '2026-06-25 13:22:51.680875', 1, NULL);
INSERT INTO public.task_history VALUES (250, 57, 2, 3, 2, '2026-06-25 13:23:00.699094', 6, 28.4);


--
-- Data for Name: task_note; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- Data for Name: task_status; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.task_status VALUES (1, 'Новая');
INSERT INTO public.task_status VALUES (2, 'В работе');
INSERT INTO public.task_status VALUES (3, 'На ревью');
INSERT INTO public.task_status VALUES (4, 'Тестирование');
INSERT INTO public.task_status VALUES (5, 'Готово');
INSERT INTO public.task_status VALUES (6, 'Отменена');


--
-- Data for Name: task_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.task_type VALUES (1, 'Разработка');
INSERT INTO public.task_type VALUES (2, 'Тестирование');
INSERT INTO public.task_type VALUES (3, 'Дизайн');
INSERT INTO public.task_type VALUES (4, 'Аналитика');
INSERT INTO public.task_type VALUES (5, 'DevOps');
INSERT INTO public.task_type VALUES (6, 'Исследование');


--
-- Data for Name: team; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.team VALUES (1, 'Конструкторское бюро', 3);
INSERT INTO public.team VALUES (2, 'Отдел АСУ ТП', 6);
INSERT INTO public.team VALUES (3, 'Отдел испытаний и диагностики', 7);


--
-- Data for Name: user_role; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.user_role VALUES (1, 'admin');
INSERT INTO public.user_role VALUES (2, 'project_manager');
INSERT INTO public.user_role VALUES (3, 'worker');


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.users VALUES (10, 'novikov', '$2b$10$a0QuO0GKkdbLdqjhLJzoqOF4GMqb3PpngoSZHr1OLXH.KqkfjZ9kC', 3, 8, true, '2026-05-17 22:53:07.488726', '2026-06-18 23:25:37.995886');
INSERT INTO public.users VALUES (8, 'morozov', '$2b$10$a0QuO0GKkdbLdqjhLJzoqOF4GMqb3PpngoSZHr1OLXH.KqkfjZ9kC', 3, 6, true, '2026-05-17 22:53:07.488726', '2026-06-18 23:27:01.983944');
INSERT INTO public.users VALUES (4, 'petrova', '$2b$10$a0QuO0GKkdbLdqjhLJzoqOF4GMqb3PpngoSZHr1OLXH.KqkfjZ9kC', 3, 2, true, '2026-05-17 22:53:07.488726', NULL);
INSERT INTO public.users VALUES (5, 'sidorov', '$2b$10$a0QuO0GKkdbLdqjhLJzoqOF4GMqb3PpngoSZHr1OLXH.KqkfjZ9kC', 3, 3, true, '2026-05-17 22:53:07.488726', NULL);
INSERT INTO public.users VALUES (7, 'fedorova', '$2b$10$a0QuO0GKkdbLdqjhLJzoqOF4GMqb3PpngoSZHr1OLXH.KqkfjZ9kC', 3, 5, true, '2026-05-17 22:53:07.488726', NULL);
INSERT INTO public.users VALUES (9, 'volkova', '$2b$10$a0QuO0GKkdbLdqjhLJzoqOF4GMqb3PpngoSZHr1OLXH.KqkfjZ9kC', 3, 7, true, '2026-05-17 22:53:07.488726', NULL);
INSERT INTO public.users VALUES (11, 'freelancer', '$2b$10$a0QuO0GKkdbLdqjhLJzoqOF4GMqb3PpngoSZHr1OLXH.KqkfjZ9kC', 3, 10, true, '2026-05-17 22:53:07.488726', NULL);
INSERT INTO public.users VALUES (3, 'ivanov', '$2b$10$a0QuO0GKkdbLdqjhLJzoqOF4GMqb3PpngoSZHr1OLXH.KqkfjZ9kC', 3, 1, true, '2026-05-17 22:53:07.488726', '2026-06-25 13:16:47.282716');
INSERT INTO public.users VALUES (1, 'admin', '$2b$10$13PyX/KRcor6myNr3acaPO.mFmvwSKr1j5RwNuAtLrMx1uNWvrJA6', 1, NULL, true, '2026-05-17 22:53:07.488726', '2026-06-25 13:21:23.071737');
INSERT INTO public.users VALUES (2, 'manager', '$2b$10$Lu9M7ysIUGUwffLjbwKKVekIm7f97f69ffDc66N6fy5xd0fnvapUy', 2, 9, true, '2026-05-17 22:53:07.488726', '2026-06-25 13:21:59.517518');
INSERT INTO public.users VALUES (12, 'userss', '$2b$10$b81zZSY1C9B2qfA9.xcZpO2yc0sP8gUxDd3h7xi1Yl8NkUtiWZ3Dy', 3, 13, false, '2026-06-15 00:35:51.153998', '2026-06-15 01:07:26.488665');
INSERT INTO public.users VALUES (6, 'kozlov', '$2b$10$a0QuO0GKkdbLdqjhLJzoqOF4GMqb3PpngoSZHr1OLXH.KqkfjZ9kC', 3, 4, true, '2026-05-17 22:53:07.488726', '2026-06-15 01:59:13.792699');


--
-- Name: employee_id_employee_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.employee_id_employee_seq', 13, true);


--
-- Name: priority_id_priority_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.priority_id_priority_seq', 4, true);


--
-- Name: project_id_project_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.project_id_project_seq', 5, true);


--
-- Name: sprint_id_sprint_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sprint_id_sprint_seq', 6, true);


--
-- Name: task_history_id_log_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.task_history_id_log_seq', 250, true);


--
-- Name: task_id_task_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.task_id_task_seq', 64, true);


--
-- Name: task_note_id_note_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.task_note_id_note_seq', 1, false);


--
-- Name: task_status_id_task_status_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.task_status_id_task_status_seq', 6, true);


--
-- Name: task_type_id_task_type_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.task_type_id_task_type_seq', 6, true);


--
-- Name: team_id_team_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.team_id_team_seq', 4, true);


--
-- Name: user_role_id_role_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.user_role_id_role_seq', 1, false);


--
-- Name: users_id_user_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_id_user_seq', 12, true);


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

\unrestrict h6cvQQOcYxPcfXotTUpOi6Ys0gEvddnRdSMsjVi060XSzccEmbGSnI1eQhVFx4g

