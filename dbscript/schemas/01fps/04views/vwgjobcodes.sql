-- View: fps.vwgjobcodes

CREATE OR REPLACE VIEW fps.vwgjobcodes AS
SELECT DISTINCT tlkpproject.parentproject,
    timecodevalid.timecode,
    timecodevalid.workgroup,
    tlkpproject.fpsyear,
    ((((tlkpproject.shorttitle)::text || ': '::text) || (tlkpjobcode.jobcodename)::text) || (testorproduct.itemdescription)::text) AS descript
   FROM (((fps.tlkpproject
     JOIN fps.timecodevalid ON (((tlkpproject.parentproject)::text = (timecodevalid.parentproject)::text) AND tlkpproject.fpsyear = timecodevalid.fpsyear))
     LEFT JOIN fps.tlkpjobcode ON ((((timecodevalid.parentproject)::text = (tlkpjobcode.parentproject)::text) AND ((timecodevalid.jobcode)::text = (tlkpjobcode.jobcode)::text) AND timecodevalid.fpsyear = tlkpjobcode.fpsyear)))
     LEFT JOIN fps.testorproduct ON (((timecodevalid.testcode)::text = (testorproduct.itemcode)::text) AND timecodevalid.fpsyear = testorproduct.fpsyear));
