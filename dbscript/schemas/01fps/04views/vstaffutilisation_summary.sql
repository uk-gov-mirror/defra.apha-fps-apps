-- View: fps.vstaffutilisation_summary

CREATE OR REPLACE VIEW fps.vstaffutilisation_summary AS
 SELECT workgroup,
    month,
    fpsyear,
    sum(chargedhours) AS sumchargedhours,
    count(DISTINCT staffid) AS nostaff,
    fthourspermonth,
    sum(hoursperweek) AS actualweekhoursavailable,
    fthoursperweek,
    sum(ztleave) AS sumztleave
   FROM fps.vstaffutilisation
  GROUP BY workgroup, month, fpsyear, fthourspermonth, fthoursperweek;
