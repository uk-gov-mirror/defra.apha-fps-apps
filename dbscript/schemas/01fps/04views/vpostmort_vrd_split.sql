-- View: fps.vpostmort_vrd_split

CREATE OR REPLACE VIEW fps.vpostmort_vrd_split AS
SELECT "right"((monthlyoutput.workgroup)::text, 2) AS location,
    monthlyoutput.month,
    ((((
        CASE qrytestspccostplan_xtab.vetr
            WHEN 0 THEN 0
            WHEN NULL::numeric THEN 0
            ELSE 1
        END)::numeric * qrytestspccostplan_xtab.labt))::double precision * sum(monthlyoutput.volume)) AS ltsplitfee,
        CASE qrytestspccostplan_xtab.vetr
            WHEN 0 THEN 0
            WHEN NULL::numeric THEN 0
            ELSE 1
        END AS ispartofltfee,
    monthlyoutput.testcode,
    monthlyoutput.fpsyear,
    sum(monthlyoutput.volume) AS totvol,
    qrytestspccostplan_xtab.labt AS ltunitcharge,
    qrytestspccostplan_xtab.vetr AS sdunitcharge,
    ((qrytestspccostplan_xtab.labt)::double precision * sum(monthlyoutput.volume)) AS ltfee,
    ((qrytestspccostplan_xtab.vetr)::double precision * sum(monthlyoutput.volume)) AS sdfee,
    (((qrytestspccostplan_xtab.labt)::double precision * sum(monthlyoutput.volume)) + ((qrytestspccostplan_xtab.vetr)::double precision * sum(monthlyoutput.volume))) AS totalfee,
    ((((tlkptestreqmt.unitprice)::numeric)::double precision * sum(monthlyoutput.volume)) - (((qrytestspccostplan_xtab.labt)::double precision * sum(monthlyoutput.volume)) + ((qrytestspccostplan_xtab.vetr)::double precision * sum(monthlyoutput.volume)))) AS "profit/loss",
    (tlkptestreqmt.unitprice)::numeric AS unitprice
   FROM (((fps.monthlyoutput
     JOIN fps.tlkptestcapability ON ((((monthlyoutput.testcode)::text = (tlkptestcapability.testcode)::text) AND ((monthlyoutput.workgroup)::text = (tlkptestcapability.workgroup)::text) AND monthlyoutput.fpsyear = tlkptestcapability.fpsyear)))
     JOIN fps.qrytestspccostplan_xtab ON (((monthlyoutput.testcode)::text = (qrytestspccostplan_xtab.testcode)::text) AND monthlyoutput.fpsyear = qrytestspccostplan_xtab.fpsyear))
     JOIN fps.tlkptestreqmt ON ((((monthlyoutput.testcode)::text = (tlkptestreqmt.testcode)::text) AND ((monthlyoutput.buyer)::text = (tlkptestreqmt.buyer)::text) AND monthlyoutput.fpsyear = tlkptestreqmt.fpsyear)))
  GROUP BY qrytestspccostplan_xtab.labt, qrytestspccostplan_xtab.vetr, monthlyoutput.workgroup, monthlyoutput.month, monthlyoutput.testcode, tlkptestcapability.planportfolio, ((tlkptestreqmt.unitprice)::numeric), monthlyoutput.fpsyear
 HAVING (((tlkptestcapability.planportfolio)::text = ANY ((ARRAY['TG0100'::citext, 'PMPORT1'::citext])::text[])) AND (monthlyoutput.month <= ( SELECT max(tblperiod.endperiod) AS month
           FROM fps.tblperiod
          WHERE (tblperiod.finalsummariesrun = '-1'::integer))))
  ORDER BY ("right"((monthlyoutput.workgroup)::text, 2)), monthlyoutput.month, monthlyoutput.testcode;
