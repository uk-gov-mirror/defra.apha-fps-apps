-- View: fps.qryjobmonth_invoices

CREATE OR REPLACE VIEW fps.qryjobmonth_invoices AS
 SELECT projectparent,
    month,
    fpsyear,
    sum(amount) AS sumofamount1,
    sum(costofwork) AS workcost
   FROM fps.proj_invoice
  GROUP BY projectparent, month, fpsyear;
