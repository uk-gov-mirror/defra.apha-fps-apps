-- View: fps.qryjobmonth_transfers2

CREATE OR REPLACE VIEW fps.qryjobmonth_transfers2 AS
SELECT DISTINCT project,
    month,
    fpsyear,
    sum(transfercost) AS sumoftransfercost
   FROM fps.qryjobmonth_transfers1
  GROUP BY project, month, fpsyear;
