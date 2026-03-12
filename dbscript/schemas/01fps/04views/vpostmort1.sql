-- View: fps.vpostmort1

CREATE OR REPLACE VIEW fps.vpostmort1 AS
SELECT tlkptestcapability.planportfolio,
    monthlyoutput.testcode,
    testorproduct.shortdescription AS itemdescription,
    monthlyoutput.fpsyear,
    sum(monthlyoutput.volume) AS totvol,
    qrytestspccostplan_xtab.labt AS ltunitcharge,
    qrytestspccostplan_xtab.vetr AS sdunitcharge,
    ((qrytestspccostplan_xtab.labt)::double precision * sum(monthlyoutput.volume)) AS ltfee,
    ((qrytestspccostplan_xtab.vetr)::double precision * sum(monthlyoutput.volume)) AS sdfee,
    (sum(monthlyoutput.volume) + ((qrytestspccostplan_xtab.vetr)::double precision * sum(monthlyoutput.volume))) AS totalfee,
    sum((((vtbltestrequ.testprice)::numeric)::double precision * monthlyoutput.volume)) AS feecharged,
    ((sum((((vtbltestrequ.testprice)::numeric)::double precision * monthlyoutput.volume)) - sum(monthlyoutput.volume)) + ((qrytestspccostplan_xtab.vetr)::double precision * sum(monthlyoutput.volume))) AS "profit/loss",
    monthlyoutput.workgroup
   FROM (((fps.vtbltestrequ
     JOIN (fps.tlkptestcapability
     JOIN fps.monthlyoutput ON ((((tlkptestcapability.workgroup)::text = (monthlyoutput.workgroup)::text) AND ((tlkptestcapability.testcode)::text = (monthlyoutput.testcode)::text) AND tlkptestcapability.fpsyear = monthlyoutput.fpsyear))) ON ((((vtbltestrequ.testcode)::text = (monthlyoutput.testcode)::text) AND ((vtbltestrequ.jobcode)::text = (monthlyoutput.buyer)::text) AND vtbltestrequ.fpsyear = monthlyoutput.fpsyear)))
     JOIN fps.testorproduct ON (((monthlyoutput.testcode)::text = (testorproduct.itemcode)::text) AND monthlyoutput.fpsyear = testorproduct.fpsyear))
     LEFT JOIN fps.qrytestspccostplan_xtab ON (((testorproduct.itemcode)::text = (qrytestspccostplan_xtab.testcode)::text) AND testorproduct.fpsyear = qrytestspccostplan_xtab.fpsyear))
  WHERE (monthlyoutput.month <= ( SELECT max(tblperiod.endperiod) AS endperiod
           FROM fps.tblperiod
          WHERE (tblperiod.finalsummariesrun = '-1'::integer)))
  GROUP BY tlkptestcapability.planportfolio, monthlyoutput.testcode, testorproduct.shortdescription, qrytestspccostplan_xtab.labt, qrytestspccostplan_xtab.vetr, monthlyoutput.workgroup, monthlyoutput.fpsyear
 HAVING ((tlkptestcapability.planportfolio)::text ~~ 'tg0100'::text);
