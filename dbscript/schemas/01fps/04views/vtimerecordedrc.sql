-- View: fps.vtimerecordedrc

CREATE OR REPLACE VIEW fps.vtimerecordedrc AS
 SELECT timecostcalcs.project,
    workgroup.profitcentre,
    timecostcalcs.fpsyear
   FROM (fps.workgroup
     JOIN fps.timecostcalcs ON ((((workgroup.workgroup)::text = (timecostcalcs.workgroup)::text) AND (workgroup.fpsyear = timecostcalcs.fpsyear))))
  GROUP BY timecostcalcs.project, workgroup.profitcentre, timecostcalcs.fpsyear;
