-- View: fps.qryrptahvggraph1

CREATE OR REPLACE VIEW fps.qryrptahvggraph1 AS
SELECT monthlyoutput.buyer,
    tblkperiodmonth.endmonth,
    monthlyoutput.testcode,
    monthlyoutput.fpsyear,
    sum(monthlyoutput.volume) AS actualvol
   FROM (fps.tlkptestcapability
     JOIN (fps.tblkperiodmonth
     JOIN fps.monthlyoutput ON ((tblkperiodmonth.monthno = monthlyoutput.month) AND tblkperiodmonth.fpsyear = monthlyoutput.fpsyear)) ON ((((tlkptestcapability.workgroup)::text = (monthlyoutput.workgroup)::text) AND ((tlkptestcapability.testcode)::text = (monthlyoutput.testcode)::text) AND tlkptestcapability.fpsyear = monthlyoutput.fpsyear)))
  GROUP BY monthlyoutput.buyer, tblkperiodmonth.endmonth, monthlyoutput.testcode, monthlyoutput.fpsyear;
