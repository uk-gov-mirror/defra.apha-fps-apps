-- View: fps.vtimecostcalcs

CREATE OR REPLACE VIEW fps.vtimecostcalcs AS
 SELECT timecostcalcs.workgroup,
    timecostcalcs.jobcode,
    timecostcalcs.project,
    timecostcalcs.month,
    timecostcalcs.staffid,
    timecostcalcs.gradecode,
    timecostcalcs.name,
    timecostcalcs.chargerate,
    timecostcalcs.class,
    timecostcalcs."time",
    timecostcalcs.cost,
    timecostcalcs.division,
    timecostcalcs.jobcodeold,
    timecostcalcs.fpsyear
   FROM (fps.timecostcalcs
     JOIN fps.vtlkpproject ON ((((timecostcalcs.project)::text = (vtlkpproject.parentproject)::text) AND (vtlkpproject.fpsyear = timecostcalcs.fpsyear))));
