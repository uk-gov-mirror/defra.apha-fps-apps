-- View: fps.qryjobmonth_tctransfers

CREATE OR REPLACE VIEW fps.qryjobmonth_tctransfers AS
SELECT vpacttlkptestcapability.planportfolio AS project,
    monthlyoutput.month,
    monthlyoutput.testcode,
    monthlyoutput.volume,
    tlkptestreqmt.unitprice AS intunitprice,
    monthlyoutput.fpsyear,
    sum((monthlyoutput.volume * tlkptestreqmt.unitprice)) AS transfercost
   FROM ((fps.monthlyoutput
     JOIN fps.tlkptestreqmt ON ((((monthlyoutput.testcode)::text = (tlkptestreqmt.testcode)::text) AND ((monthlyoutput.buyer)::text = (tlkptestreqmt.buyer)::text) AND monthlyoutput.fpsyear = tlkptestreqmt.fpsyear)))
     JOIN fps.vpacttlkptestcapability ON (((tlkptestreqmt.buyer)::text = vpacttlkptestcapability.wgtestcode) AND tlkptestreqmt.fpsyear = vpacttlkptestcapability.fpsyear))
  GROUP BY vpacttlkptestcapability.planportfolio, monthlyoutput.month, monthlyoutput.testcode, monthlyoutput.volume, tlkptestreqmt.unitprice, monthlyoutput.fpsyear;
