--liquibase formatted sql

--changeset repo-admin:008_fps_qrytotaltestcosts labels:ddl context:all

DROP VIEW fps.qrytotaltestcosts;
CREATE OR REPLACE VIEW fps.qrytotaltestcosts AS
SELECT
    tr.jobcode,
    tr.fpsyear,
    SUM(tr.notests * tr.testprice) AS totaltestcosts
FROM fps.vtbltestrequ tr
JOIN fps.tlkpproject p
    ON p.parentproject = tr.jobcode
   AND p.fpsyear = tr.fpsyear
GROUP BY tr.jobcode, tr.fpsyear;

--ROLLBACK
--Not Applicable