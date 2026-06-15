
--liquibase formatted sql

--changeset repo-admin:004_fps_vpvtprojectgroupmgrplan labels:ddl context: all


CREATE OR REPLACE VIEW fps.vpvtprojectgroupmgrplan AS
SELECT DISTINCT
    p.projectgroup,
    p.fpsyear,
    sj.useremail,
    pcg.profitcentre                                              AS resourcecentre,
    wgg.workgroup,
    wgg.gradecode,
    COALESCE(e.lastname, '')  ', '  COALESCE(e.firstname, '') AS name,
    p.manager,
    sj.jobcode,
    p.projectstatus,
    sj.plannedhours                                               AS hrs,
    pcg.chargerate,
    sj.plannedhours
         CASE
              WHEN prog.sector_name = 'charge'
              THEN 1numeric
              ELSE 0numeric
          ENDdouble precision
         pcg.chargerate AS fee
FROM fps.tlkpproject p
JOIN fps.vtblstaffjob_bygroup sj
    ON sj.jobcode = p.parentproject
   AND sj.fpsyear = p.fpsyear
JOIN fps.tblwgemployee wge
    ON wge.pactid = sj.staffid
   AND wge.fpsyear = sj.fpsyear
JOIN fps.tblemployee e
    ON e.spnumber = wge.spnumber
   AND e.fpsyear = wge.fpsyear
JOIN fps.workgroupgrade wgg
    ON wgg.wggrade = wge.workgroupgrade
   AND wgg.fpsyear = wge.fpsyear
JOIN fps.profitcentregrade pcg
    ON pcg.pcgrade = wgg.profitcentregrade
   AND pcg.fpsyear = wgg.fpsyear
JOIN fps.tlkpprogram prog
    ON prog.programno = p.program
   AND prog.fpsyear = p.fpsyear

--rollback DROP VIEW fps.vpvtprojectgroupmgrplan;