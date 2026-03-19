-- View: fps.qryjobmonthportfoliosales

CREATE OR REPLACE VIEW fps.qryjobmonthportfoliosales AS
 SELECT DISTINCT tlkptestcapability.planportfolio,
    monthlyoutput.month,
    monthlyoutput.fpsyear,
    sum((tlkptestreqmt.unitprice * monthlyoutput.volume)) AS fee
   FROM (fps.tlkptestreqmt
     JOIN (fps.tlkptestcapability
     JOIN fps.monthlyoutput ON ((((tlkptestcapability.workgroup)::text = (monthlyoutput.workgroup)::text) AND ((tlkptestcapability.testcode)::text = (monthlyoutput.testcode)::text) AND (tlkptestcapability.fpsyear = monthlyoutput.fpsyear)))) ON ((((tlkptestreqmt.buyer)::text = (monthlyoutput.buyer)::text) AND ((tlkptestreqmt.testcode)::text = (monthlyoutput.testcode)::text) AND (tlkptestreqmt.fpsyear = monthlyoutput.fpsyear))))
  GROUP BY tlkptestcapability.planportfolio, monthlyoutput.month, monthlyoutput.fpsyear;
