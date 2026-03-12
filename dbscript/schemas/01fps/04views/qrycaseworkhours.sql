-- View: fps.qrycaseworkhours
CREATE OR REPLACE VIEW fps.qrycaseworkhours AS
 SELECT timecostcalcs.project,
    timecostcalcs.month,
    timecostcalcs.fpsyear,
    sum(timecostcalcs.cost) AS sumofcost
   FROM (fps.timecostcalcs
     JOIN fps.tlkpjobcode ON ((((timecostcalcs.project)::text = (tlkpjobcode.parentproject)::text) AND ((timecostcalcs.jobcode)::text = (tlkpjobcode.jobcode)::text) AND timecostcalcs.fpsyear = tlkpjobcode.fpsyear)))
  WHERE ((tlkpjobcode.type)::text = 'casework'::text)
  GROUP BY timecostcalcs.project, timecostcalcs.month, timecostcalcs.fpsyear;
