-- View: fps.vtimecostcalcs_allstaff

CREATE OR REPLACE VIEW fps.vtimecostcalcs_allstaff AS
SELECT timecostcalcs.workgroup,
    timecostcalcs.month,
    timecostcalcs.staffid,
    timecostcalcs.project,
    timecostcalcs.gradecode,
    timecostcalcs.name,
    timecostcalcs.class,
    timecostcalcs."time",
    timecostcalcs.fpsyear
   FROM fps.timecostcalcs
UNION ALL
 SELECT workgroupgrade.workgroup,
    tblperiod.endperiod AS month,
    vtblstaff_general.staffid,
    ''::character varying AS project,
    workgroupgrade.gradecode,
    vtblstaff_general.name,
    ''::character varying AS class,
    0 AS "time",
    vtblstaff_general.fpsyear
   FROM ((fps.vtblstaff_general
     JOIN fps.workgroupgrade ON (((vtblstaff_general.workgroupgrade)::text = (workgroupgrade.wggrade)::text) AND vtblstaff_general.fpsyear = workgroupgrade.fpsyear))
     CROSS JOIN fps.tblperiod)
  WHERE ((tblperiod.finalsummariesrun = '-1'::integer) AND (vtblstaff_general.name !~~ '%general'::text));
