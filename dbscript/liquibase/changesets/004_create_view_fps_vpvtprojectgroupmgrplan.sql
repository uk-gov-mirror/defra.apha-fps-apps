--liquibase formatted sql

--changeset repo-admin:004_fps_vpvtprojectgroupmgrplan labels:ddl context:all

CREATE OR REPLACE VIEW fps.vpvtprojectgroupmgrplan
 AS
 SELECT DISTINCT p.projectgroup,
    p.fpsyear,
    sj.useremail,
    pcg.profitcentre AS resourcecentre,
    wgg.workgroup,
    wgg.gradecode,
    (COALESCE(e.lastname, ''::character varying)::text || ', '::text) || COALESCE(e.firstname, ''::character varying)::text AS name,
    p.manager,
    sj.jobcode,
    p.projectstatus,
    sj.plannedhours AS hrs,
    pcg.chargerate,
    sj.plannedhours *
        CASE
            WHEN lower(prog.sector_name::text) = 'charge'::text THEN 1::numeric
            ELSE 0::numeric
        END::double precision * pcg.chargerate AS fee
   FROM fps.tlkpproject p
     JOIN fps.vtblstaffjob_bygroup sj ON sj.jobcode::text = p.parentproject::text AND sj.fpsyear = p.fpsyear
     JOIN fps.tblwgemployee wge ON wge.pactid::text = sj.staffid::text AND wge.fpsyear = sj.fpsyear
     JOIN fps.tblemployee e ON e.spnumber::text = wge.spnumber::text AND e.fpsyear = wge.fpsyear
     JOIN fps.workgroupgrade wgg ON wgg.wggrade::text = wge.workgroupgrade::text AND wgg.fpsyear = wge.fpsyear
     JOIN fps.profitcentregrade pcg ON pcg.pcgrade::text = wgg.profitcentregrade::text AND pcg.fpsyear = wgg.fpsyear
     JOIN fps.tlkpprogram prog ON prog.programno::text = p.program::text AND prog.fpsyear = p.fpsyear;

--rollback DROP VIEW fps.vpvtprojectgroupmgrplan;