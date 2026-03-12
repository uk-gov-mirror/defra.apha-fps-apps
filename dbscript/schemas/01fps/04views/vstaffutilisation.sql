-- View: fps.vstaffutilisation

CREATE OR REPLACE VIEW fps.vstaffutilisation AS
SELECT vstaffutilisation_time.workgroup,
    tlkpmonthhours.month,
    tlkpmonthhours.cvlhours AS fthourspermonth,
    vstaffutilisation_time.fthoursperweek,
    vstaffutilisation_time.name,
    vstaffutilisation_time.staffid,
    vstaffutilisation_time.gradecode,
    vstaffutilisation_time.timerecorder,
    vstaffutilisation_time.fpsyear,
    ((vstaffutilisation_time.hoursperweek * (fps.fnproratapartmonth(vstaffutilisation_time.startdate, vstaffutilisation_time.enddate, tlkpmonthhours.month, tlkpmonthhours.year))::double precision))::numeric(9,2) AS hoursperweek,
    vstaffutilisation_time.hrspaid,
    sum(
        CASE vtimecostcalcs_allstaff.project
            WHEN 'ZTLeave'::text THEN (0)::double precision
            WHEN 'ZTWork'::text THEN (0)::double precision
            ELSE vtimecostcalcs_allstaff."time"
        END) AS chargedhours,
    sum(
        CASE vtimecostcalcs_allstaff.project
            WHEN 'ZTLeave'::text THEN vtimecostcalcs_allstaff."time"
            ELSE ((0)::numeric)::double precision
        END) AS ztleave,
    sum(
        CASE vtimecostcalcs_allstaff.project
            WHEN 'ZTWork'::text THEN vtimecostcalcs_allstaff."time"
            ELSE ((0)::numeric)::double precision
        END) AS ztwork
   FROM ((fps.tlkpmonthhours
     LEFT JOIN fps.vtimecostcalcs_allstaff ON (((tlkpmonthhours.fmonth)::double precision = vtimecostcalcs_allstaff.month) AND tlkpmonthhours.fpsyear = vtimecostcalcs_allstaff.fpsyear))
     RIGHT JOIN fps.vstaffutilisation_time ON (((vtimecostcalcs_allstaff.staffid)::text = (vstaffutilisation_time.staffid)::text) AND vtimecostcalcs_allstaff.fpsyear = vstaffutilisation_time.fpsyear))
  GROUP BY vstaffutilisation_time.workgroup, tlkpmonthhours.month, tlkpmonthhours.cvlhours, vstaffutilisation_time.name, vstaffutilisation_time.gradecode, vstaffutilisation_time.hrspaid, vstaffutilisation_time.timerecorder, vstaffutilisation_time.fthoursperweek, vstaffutilisation_time.staffid, vstaffutilisation_time.fpsyear, (((vstaffutilisation_time.hoursperweek * (fps.fnproratapartmonth(vstaffutilisation_time.startdate, vstaffutilisation_time.enddate, tlkpmonthhours.month, tlkpmonthhours.year))::double precision))::numeric(9,2));
