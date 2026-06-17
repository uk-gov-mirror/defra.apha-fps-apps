
--liquibase formatted sql

--changeset repo-admin:008_fps_qrytotaltestcosts labels:ddl context:all

DROP VIEW fps.qrytotaltestcosts;
CREATE OR REPLACE VIEW fps.qrytotaltestcosts AS 
SELECT tr.jobcode, p.fpsyear, SUM(tr.notests * tr.testprice) AS totaltestcosts 
FROM fps.vtbltestrequ tr INNER JOIN fps.tlkpproject p ON p.parentproject = tr.jobcode 
GROUP BY tr.jobcode, p.fpsyear;

--ROLLBACK 
-- Not applicable