-- View: fps.qrytcc_union

CREATE OR REPLACE VIEW fps.qrytcc_union AS
SELECT workgroup.workgroup,
    tlkpproject.parentproject AS project,
    tlkpproject.fpsyear
   FROM fps.workgroup
   JOIN fps.tlkpproject ON workgroup.fpsyear = tlkpproject.fpsyear
  WHERE ((workgroup.workgroup)::text ~~ 'SV__'::text)
UNION
 SELECT timecostcalcs.workgroup,
    timecostcalcs.project,
    timecostcalcs.fpsyear
   FROM fps.timecostcalcs;
