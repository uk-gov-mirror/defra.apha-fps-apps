-- View: fps.vworkgroupratehours

CREATE OR REPLACE VIEW fps.vworkgroupratehours AS
 SELECT DISTINCT timecostcalcs_1.workgroup,
    timecostcalcs.chargerate AS rate,
    tblmtconversion.hours,
    timecostcalcs.fpsyear
   FROM (((fps.timecostcalcs
     JOIN fps.timecodevalid ON ((((timecostcalcs.workgroup)::text = (timecodevalid.workgroup)::text) AND (timecostcalcs.fpsyear = timecodevalid.fpsyear))))
     JOIN fps.timecostcalcs timecostcalcs_1 ON ((((timecodevalid.parentproject)::text = (timecostcalcs_1.project)::text) AND (timecodevalid.fpsyear = timecostcalcs_1.fpsyear))))
     JOIN fps.tblmtconversion ON (((timecostcalcs_1.project)::text = (tblmtconversion.newproject)::text)))
  WHERE (((timecodevalid.parentproject)::text = 'TG0100'::text) AND ((timecostcalcs.staffid)::text = '4464'::text));
