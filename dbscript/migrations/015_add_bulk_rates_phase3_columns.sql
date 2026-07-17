-- Migration 015: Add Phase 3 Bulk Rates columns to fps.job_queue
-- Adds cancellation, trigger, and audit columns introduced in Phase 3 development.
-- Columns that already exist (configuration_json, approved_*, rejected_*) are guarded with IF NOT EXISTS.

-- ── fps.job_queue additions ──────────────────────────────────────────────────

ALTER TABLE fps.job_queue
    ADD COLUMN IF NOT EXISTS configuration_json  jsonb,
    ADD COLUMN IF NOT EXISTS approved_by         text,
    ADD COLUMN IF NOT EXISTS approved_at_utc     timestamptz,
    ADD COLUMN IF NOT EXISTS rejected_by         text,
    ADD COLUMN IF NOT EXISTS rejected_at_utc     timestamptz,
    ADD COLUMN IF NOT EXISTS rejection_reason    text,
    ADD COLUMN IF NOT EXISTS cancelled_by        text,
    ADD COLUMN IF NOT EXISTS cancelled_at_utc    timestamptz,
    ADD COLUMN IF NOT EXISTS cancellation_reason text,
    ADD COLUMN IF NOT EXISTS triggered_by        text,
    ADD COLUMN IF NOT EXISTS triggered_at_utc    timestamptz,
    ADD COLUMN IF NOT EXISTS updated_at          timestamptz,
    ADD COLUMN IF NOT EXISTS startdatetime       timestamptz,
    ADD COLUMN IF NOT EXISTS enddatetime         timestamptz,
    ADD COLUMN IF NOT EXISTS errormessage        text;

-- ── fps.job_queue_log ────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS fps.job_queue_log (
    jobqueuelogid  bigserial    PRIMARY KEY,
    jobqueueid     uuid         NOT NULL REFERENCES fps.job_queue(jobqueueid),
    statusid       int          NOT NULL,
    performedby    text,
    logtime        timestamptz  NOT NULL DEFAULT now(),
    note           text
);

-- ── fps.tblstagingtestorproduct ──────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS fps.tblstagingtestorproduct (
    id              bigserial    PRIMARY KEY,
    jobqueueid      uuid         NOT NULL REFERENCES fps.job_queue(jobqueueid),
    testcode        text         NOT NULL,
    unitpricevla    numeric(18,4),
    defraunitprice  numeric(18,4),
    fecnewrate      numeric(18,4),
    change          numeric(18,4),
    itemdescription text,
    shortdescription text,
    owner           text,
    comments        text
);

-- ── fps.tblstagingtlkptestreqmt ──────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS fps.tblstagingtlkptestreqmt (
    id          bigserial   PRIMARY KEY,
    jobqueueid  uuid        NOT NULL REFERENCES fps.job_queue(jobqueueid),
    testcode    text        NOT NULL,
    buyer       text        NOT NULL,
    agrup       numeric(18,4),
    agrupnew    numeric(18,4),
    change      numeric(18,4),
    norequired  float8,
    datecreated timestamptz,
    active      smallint,
    comments    text
);

-- ── fps.tblstagingprofitcentregrade ──────────────────────────────────────────

CREATE TABLE IF NOT EXISTS fps.tblstagingprofitcentregrade (
    id          bigserial   PRIMARY KEY,
    jobqueueid  uuid        NOT NULL REFERENCES fps.job_queue(jobqueueid),
    pcgrade     text        NOT NULL,
    payrate     numeric(18,4),
    npr         numeric(18,4),
    ohr         numeric(18,4)
);

-- ── fps.tblstaginganimals ────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS fps.tblstaginganimals (
    id             bigserial   PRIMARY KEY,
    jobqueueid     uuid        NOT NULL REFERENCES fps.job_queue(jobqueueid),
    animaltype     text        NOT NULL,
    species        text,
    security_level text,
    dailyrate      numeric(18,4),
    defradailyrate numeric(18,4),
    planbyweek     boolean
);

-- ── fps.staging_validation_error ─────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS fps.staging_validation_error (
    id                bigserial   PRIMARY KEY,
    jobqueueid        uuid        NOT NULL REFERENCES fps.job_queue(jobqueueid),
    upload_version    int         NOT NULL DEFAULT 1,
    source_row_number int         NOT NULL,
    field_name        text,
    validation_code   text,
    severity          text        NOT NULL,
    validation_message text       NOT NULL
);
