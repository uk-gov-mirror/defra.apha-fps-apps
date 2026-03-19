-- View: fps.qryjobmonth_transfers1

CREATE OR REPLACE VIEW fps.qryjobmonth_transfers1 AS
 SELECT DISTINCT monthlyoutput.buyer AS project,
    monthlyoutput.month,
    monthlyoutput.testcode,
    monthlyoutput.volume,
    tlkptestreqmt.unitprice AS intunitprice,
    monthlyoutput.fpsyear,
    sum((monthlyoutput.volume * tlkptestreqmt.unitprice)) AS transfercost
   FROM ((fps.testorproduct
     JOIN fps.tlkptestreqmt ON ((((testorproduct.itemcode)::text = (tlkptestreqmt.testcode)::text) AND (testorproduct.fpsyear = tlkptestreqmt.fpsyear))))
     JOIN fps.monthlyoutput ON ((((tlkptestreqmt.buyer)::text = (monthlyoutput.buyer)::text) AND ((tlkptestreqmt.testcode)::text = (monthlyoutput.testcode)::text) AND (tlkptestreqmt.fpsyear = monthlyoutput.fpsyear))))
  GROUP BY monthlyoutput.buyer, monthlyoutput.month, monthlyoutput.testcode, monthlyoutput.volume, tlkptestreqmt.unitprice, monthlyoutput.fpsyear;
