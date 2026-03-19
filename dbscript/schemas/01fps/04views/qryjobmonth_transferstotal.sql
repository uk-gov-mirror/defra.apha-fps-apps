-- View: fps.qryjobmonth_transferstotal

CREATE OR REPLACE VIEW fps.qryjobmonth_transferstotal AS
 SELECT DISTINCT project,
    month,
    fpsyear,
    sum(transfercost) AS sumoftransfercost
   FROM fps.qryjobmonth_transferunion
  GROUP BY project, month, fpsyear;
