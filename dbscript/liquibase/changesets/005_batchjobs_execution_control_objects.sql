--liquibase formatted sql

--changeset arihant.jain:CR009 labels:CR009 context:all
--comment: CR009 - BatchJobs execution control objects (job_master, job_status, job_queue, job_queue_log, job_lock, job_cancellation_request)

CREATE EXTENSION IF NOT EXISTS pgcrypto;


-- =========================================
-- 1) Job master
-- =========================================
CREATE TABLE IF NOT EXISTS fps.job_master (
    jobid       INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobname     VARCHAR(100) NOT NULL UNIQUE,
    frequency   VARCHAR(50),
    note        VARCHAR(250),
    timetolive  INTEGER NOT NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_job_master_timetolive_positive CHECK (timetolive > 0)
);


-- =========================================
-- 2) Job status
-- =========================================
CREATE TABLE IF NOT EXISTS fps.job_status (
    statusid    INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobid       INTEGER NOT NULL,
    status      VARCHAR(100) NOT NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_job_status_jobid
        FOREIGN KEY (jobid)
        REFERENCES fps.job_master(jobid)
        ON DELETE CASCADE,
    CONSTRAINT uq_job_status_jobid_status UNIQUE (jobid, status)
);

CREATE INDEX IF NOT EXISTS idx_job_status_jobid
    ON fps.job_status (jobid);


-- =========================================
-- 3) Job queue - execution header
-- =========================================
CREATE TABLE IF NOT EXISTS fps.job_queue (
    jobqueueid         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    jobexecutionid     UUID NOT NULL,
    jobid              INTEGER NOT NULL,
    statusid           INTEGER NOT NULL,
    requestedby        VARCHAR(256) NOT NULL,
    requested_at_utc   TIMESTAMPTZ,
    startdatetime      TIMESTAMPTZ NOT NULL,
    enddatetime        TIMESTAMPTZ,
    errormessage       VARCHAR(1000),
    created_at         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_job_queue_jobid
        FOREIGN KEY (jobid)
        REFERENCES fps.job_master(jobid)
        ON DELETE RESTRICT,
    CONSTRAINT fk_job_queue_statusid
        FOREIGN KEY (statusid)
        REFERENCES fps.job_status(statusid)
        ON DELETE RESTRICT,
    CONSTRAINT chk_job_queue_end_after_start CHECK (
        enddatetime IS NULL OR enddatetime >= startdatetime
    )
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_job_queue_jobexecutionid
    ON fps.job_queue (jobexecutionid);

CREATE INDEX IF NOT EXISTS idx_job_queue_requestedby
    ON fps.job_queue (requestedby);

CREATE INDEX IF NOT EXISTS idx_job_queue_requested_at_utc
    ON fps.job_queue (requested_at_utc);

CREATE INDEX IF NOT EXISTS idx_job_queue_jobid_startdatetime
    ON fps.job_queue (jobid, startdatetime DESC);

CREATE INDEX IF NOT EXISTS idx_job_queue_statusid
    ON fps.job_queue (statusid);


-- =========================================
-- 4) Job queue log - execution timeline
-- =========================================
CREATE TABLE IF NOT EXISTS fps.job_queue_log (
    jobqueuelogid  INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobqueueid     UUID NOT NULL,
    statusid       INTEGER NOT NULL,
    performedby    VARCHAR(256) NOT NULL,
    logtime        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    note           VARCHAR(500),
    CONSTRAINT fk_job_queue_log_jobqueueid
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue(jobqueueid)
        ON DELETE CASCADE,
    CONSTRAINT fk_job_queue_log_statusid
        FOREIGN KEY (statusid)
        REFERENCES fps.job_status(statusid)
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_job_queue_log_jobqueueid_logtime
    ON fps.job_queue_log (jobqueueid, logtime DESC);


-- =========================================
-- 5) Job lock
-- =========================================
CREATE TABLE IF NOT EXISTS fps.job_lock (
    lock_id      INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    job_name     VARCHAR(255) NOT NULL,
    acquired_at  TIMESTAMPTZ NOT NULL,
    expires_at   TIMESTAMPTZ NOT NULL,
    jobqueueid   UUID NULL,
    is_active    BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_job_lock_job_name
    ON fps.job_lock (job_name);

CREATE INDEX IF NOT EXISTS idx_job_lock_job_name_active
    ON fps.job_lock (job_name, is_active);

CREATE INDEX IF NOT EXISTS idx_job_lock_expires_at
    ON fps.job_lock (expires_at);

CREATE UNIQUE INDEX IF NOT EXISTS uq_job_lock_job_name_active
    ON fps.job_lock (job_name)
    WHERE is_active = TRUE;


-- =========================================
-- 6) Durable cancellation requests
-- =========================================
CREATE TABLE IF NOT EXISTS fps.job_cancellation_request (
    jobexecutionid      UUID PRIMARY KEY,
    requested_by        VARCHAR(256) NOT NULL,
    requested_at_utc    TIMESTAMPTZ NOT NULL,
    status              VARCHAR(50) NOT NULL,
    source              VARCHAR(100),
    consumed_at_utc     TIMESTAMPTZ,
    consumed_by         VARCHAR(256),
    terminalized_at_utc TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS idx_job_cancel_requested_at
    ON fps.job_cancellation_request (requested_at_utc);

CREATE INDEX IF NOT EXISTS idx_job_cancel_status
    ON fps.job_cancellation_request (status);

--rollback DROP TABLE IF EXISTS fps.job_cancellation_request;
--rollback DROP TABLE IF EXISTS fps.job_lock;
--rollback DROP TABLE IF EXISTS fps.job_queue_log;
--rollback DROP TABLE IF EXISTS fps.job_queue;
--rollback DROP TABLE IF EXISTS fps.job_status;
--rollback DROP TABLE IF EXISTS fps.job_master;
--rollback DROP SCHEMA IF EXISTS fps;
--rollback DROP EXTENSION IF EXISTS pgcrypto;
