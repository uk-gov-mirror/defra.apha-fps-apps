-- View: fps.vtestssummary

CREATE OR REPLACE VIEW fps.vtestssummary AS
SELECT tlkpprogram.programno,
    tlkpproject.parentproject,
    tlkptestreqmt.testcode,
    testorproduct.itemdescription,
    tlkptestreqmt.fpsyear,
    (tlkptestreqmt.norequired * tlkptestreqmt.unitprice) AS "planned test cost",
    tlkptestreqmt.norequired AS "planned test vol",
    sum(monthlyoutput.volume) AS "brought test volume",
    sum((monthlyoutput.volume * tlkptestreqmt.unitprice)) AS "brought test cost"
   FROM ((((fps.tlkpprogram
     JOIN fps.tlkpproject ON (((tlkpprogram.programno)::text = (tlkpproject.program)::text) AND tlkpprogram.fpsyear = tlkpproject.fpsyear))
     JOIN fps.tlkptestreqmt ON (((tlkpproject.parentproject)::text = (tlkptestreqmt.buyer)::text) AND tlkpproject.fpsyear = tlkptestreqmt.fpsyear))
     LEFT JOIN fps.monthlyoutput ON ((((tlkptestreqmt.buyer)::text = (monthlyoutput.buyer)::text) AND ((tlkptestreqmt.testcode)::text = (monthlyoutput.testcode)::text) AND tlkptestreqmt.fpsyear = monthlyoutput.fpsyear)))
     JOIN fps.testorproduct ON (((tlkptestreqmt.testcode)::text = (testorproduct.itemcode)::text) AND tlkptestreqmt.fpsyear = testorproduct.fpsyear))
  GROUP BY tlkpprogram.programno, tlkpproject.parentproject, tlkptestreqmt.testcode, testorproduct.itemdescription, tlkptestreqmt.fpsyear, (tlkptestreqmt.norequired * tlkptestreqmt.unitprice), tlkptestreqmt.norequired;
