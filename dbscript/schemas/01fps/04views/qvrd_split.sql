-- View: fps.qvrd_split

CREATE OR REPLACE VIEW fps.qvrd_split AS
 SELECT monthlyoutput.workgroup AS location,
    monthlyoutput.month,
    ((
        CASE vplancrosstab.vetr
            WHEN 0 THEN (1)::numeric
            ELSE (0)::numeric
        END)::double precision * ((vplancrosstab.labt)::double precision * sum(monthlyoutput.volume))) AS ltsplitfee,
        CASE vplancrosstab.vetr
            WHEN 0 THEN (1)::numeric
            ELSE (0)::numeric
        END AS ispartofltfee,
    monthlyoutput.testcode,
    monthlyoutput.fpsyear,
    sum(monthlyoutput.volume) AS totvol,
    vplancrosstab.labt AS ltunitcharge,
    vplancrosstab.vetr AS sdunitcharge,
    ((vplancrosstab.labt)::double precision * sum(monthlyoutput.volume)) AS ltfee,
    ((vplancrosstab.vetr)::double precision * sum(monthlyoutput.volume)) AS sdfee,
    (((vplancrosstab.labt)::double precision * sum(monthlyoutput.volume)) + ((vplancrosstab.vetr)::double precision * sum(monthlyoutput.volume))) AS totalfee,
    ((((tlkptestreqmt.unitprice)::numeric)::double precision * sum(monthlyoutput.volume)) - (((vplancrosstab.labt)::double precision * sum(monthlyoutput.volume)) + ((vplancrosstab.vetr)::double precision * sum(monthlyoutput.volume)))) AS "profit/loss",
    (tlkptestreqmt.unitprice)::numeric AS testprice
   FROM (((fps.monthlyoutput
     JOIN fps.tlkptestcapability ON ((((monthlyoutput.testcode)::text = (tlkptestcapability.testcode)::text) AND ((monthlyoutput.workgroup)::text = (tlkptestcapability.workgroup)::text) AND (monthlyoutput.fpsyear = tlkptestcapability.fpsyear))))
     JOIN fps.vplancrosstab ON ((((monthlyoutput.testcode)::text = (vplancrosstab.testcode)::text) AND (monthlyoutput.fpsyear = vplancrosstab.fpsyear))))
     JOIN fps.tlkptestreqmt ON ((((monthlyoutput.testcode)::text = (tlkptestreqmt.testcode)::text) AND ((monthlyoutput.buyer)::text = (tlkptestreqmt.buyer)::text) AND (monthlyoutput.fpsyear = tlkptestreqmt.fpsyear))))
  GROUP BY vplancrosstab.labt, vplancrosstab.vetr, monthlyoutput.workgroup, monthlyoutput.month, monthlyoutput.testcode, tlkptestcapability.planportfolio, ((tlkptestreqmt.unitprice)::numeric), monthlyoutput.fpsyear
 HAVING (((tlkptestcapability.planportfolio)::text = 'TG0100'::text) AND (monthlyoutput.month <= ( SELECT max(tblperiod.endperiod) AS month
           FROM fps.tblperiod
          WHERE (tblperiod.finalsummariesrun = '-1'::integer))) AND (vplancrosstab.labt IS NOT NULL) AND (vplancrosstab.vetr IS NOT NULL))
  ORDER BY monthlyoutput.workgroup, monthlyoutput.month, monthlyoutput.testcode;
