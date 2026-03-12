-- View: fps.qryjobmonth_subcontracts

CREATE OR REPLACE VIEW fps.qryjobmonth_subcontracts AS
SELECT project,
    month,
    fpsyear,
    sum(animals1) AS animals,
    sum(other1) AS other,
    (sum(animals1) + sum(other1)) AS total
   FROM fps.qryjobmonth_subcontracts1
  GROUP BY project, month, fpsyear;
