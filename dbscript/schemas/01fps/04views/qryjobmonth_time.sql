-- View: fps.qryjobmonth_time

CREATE OR REPLACE VIEW fps.qryjobmonth_time AS
 SELECT DISTINCT project,
    month,
    fpsyear,
    sum(cost) AS sumofcost,
    sum("time") AS sumofhours,
    sum(pay) AS sumofpayrate
   FROM fps.timecostcalcs
  GROUP BY project, month, fpsyear;
