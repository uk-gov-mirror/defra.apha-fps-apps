-- View: fps.qrysurvprojectsum
CREATE OR REPLACE VIEW fps.qrysurvprojectsum AS
 SELECT DISTINCT qrytcc_union.project,
    qrytcc_union.workgroup,
    tlkpjobcode.type,
    sum(timecostcalcs.cost) AS hourscost,
    timecostcalcs.month,
    timecostcalcs.fpsyear
   FROM (fps.qrytcc_union
     LEFT JOIN (fps.tlkpjobcode
     RIGHT JOIN fps.timecostcalcs ON ((((tlkpjobcode.parentproject)::text = (timecostcalcs.project)::text) AND ((tlkpjobcode.jobcode)::text = (timecostcalcs.jobcode)::text) AND tlkpjobcode.fpsyear = timecostcalcs.fpsyear))) ON ((((qrytcc_union.project)::text = (timecostcalcs.project)::text) AND ((qrytcc_union.workgroup)::text = (timecostcalcs.workgroup)::text) AND qrytcc_union.fpsyear = timecostcalcs.fpsyear)))
  GROUP BY qrytcc_union.project, qrytcc_union.workgroup, tlkpjobcode.type, timecostcalcs.month, timecostcalcs.fpsyear;
