--liquibase formatted sql

--changeset arihant.jain:CR010 labels:CR010 context:all
--comment: CR010 - Seed BatchJobs job master and execution statuses

-- =========================================
-- 1) Seed job master
-- =========================================
INSERT INTO fps.job_master (jobname, frequency, note, timetolive, created_at, updated_at)
VALUES
    ('RecreateSummary', 'Manual',            'Recreate Summay',                       3600, NOW(), NOW()),
    ('MABArchive',      'Mon-Fri 20:00 UTC', 'MAB Archive - scheduled weekday 8pm',  3600, NOW(), NOW()),
    ('FECProcess',      'Manual',            'FEC Process',                           3600, NOW(), NOW()),
    ('YearEndProcess',  'Manual',            'YearEnd Process',                       3600, NOW(), NOW())
ON CONFLICT (jobname) DO UPDATE
SET
    frequency  = EXCLUDED.frequency,
    note       = EXCLUDED.note,
    timetolive = EXCLUDED.timetolive,
    updated_at = NOW();

-- =========================================
-- 2) Seed execution statuses per job
-- Lifecycle: Initiated -> Running -> Completed|Failed|Cancelled
-- =========================================
INSERT INTO fps.job_status (jobid, status, created_at)
SELECT jm.jobid, s.status_value, NOW()
FROM fps.job_master jm
JOIN (
    VALUES
        ('RecreateSummary'),
        ('MABArchive'),
        ('FECProcess'),
        ('YearEndProcess')
) AS target_jobs(jobname)
    ON target_jobs.jobname = jm.jobname
CROSS JOIN (
    VALUES
        ('Initiated'),
        ('Running'),
        ('Completed'),
        ('Failed'),
        ('Cancelled')
) AS s(status_value)
ON CONFLICT (jobid, status) DO NOTHING;

-- =========================================
-- 3) Verification
-- =========================================
SELECT jobid, jobname, frequency, note, timetolive
FROM fps.job_master
ORDER BY jobname;

SELECT jm.jobname, jm.frequency, js.status, js.statusid
FROM fps.job_master jm
JOIN fps.job_status js ON js.jobid = jm.jobid
ORDER BY jm.jobname, js.status;

--rollback DELETE FROM fps.job_status WHERE jobid IN (SELECT jobid FROM fps.job_master WHERE jobname IN ('RecreateSummary', 'MABArchive', 'FECProcess', 'YearEndProcess'));
--rollback DELETE FROM fps.job_master WHERE jobname IN ('RecreateSummary', 'MABArchive', 'FECProcess', 'YearEndProcess');
