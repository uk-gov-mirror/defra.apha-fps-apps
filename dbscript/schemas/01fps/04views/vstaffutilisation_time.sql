-- View: fps.vstaffutilisation_time

CREATE OR REPLACE VIEW fps.vstaffutilisation_time AS
 SELECT tblwgemployee.pactid AS staffid,
    (((COALESCE(tblemployee.lastname, ''::character varying))::text || ', '::text) || (COALESCE(tblemployee.firstname, ''::character varying))::text) AS name,
    workgroupgrade.workgroup,
    workgroupgrade.gradecode,
    tblwgemployee.workgroupgrade,
    tblemployee.title,
    tblwgemployee.personstatus,
    tblwgemployee.personclass,
    tblwgemployee.hrspaid,
    tblwgemployee.timerecorder,
    tblwgemployee.hoursperweek,
    vfthours.fthoursperday,
    vfthours.fthoursperweek,
    tblwgemployee.startdate,
    tblwgemployee.enddate,
    tblwgemployee.fpsyear
   FROM (((fps.tblwgemployee
     JOIN fps.tblemployee ON (((tblwgemployee.spnumber)::text = (tblemployee.spnumber)::text)))
     JOIN fps.workgroupgrade ON (((tblwgemployee.workgroupgrade)::text = (workgroupgrade.wggrade)::text)))
     CROSS JOIN fps.vfthours)
  WHERE ((tblemployee.firstname)::text <> 'General'::text);
