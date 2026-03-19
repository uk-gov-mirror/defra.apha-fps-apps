-- View: fps.qrytestspccostplan_crosstab

CREATE OR REPLACE VIEW fps.qrytestspccostplan_crosstab AS
 SELECT qrytestpcccostplan.jobcode,
    qrytestpcccostplan.testcode,
    qrytestpcccostplan.profitcentre,
    qrytestpcccostplan.fpsyear,
        CASE qrytestpcccostplan.profitcentre
            WHEN 'LabT'::text THEN max(COALESCE((tbltestrequirementrccost.price)::numeric, qrytestpcccostplan.price))
            ELSE (0)::numeric
        END AS labt,
        CASE qrytestpcccostplan.profitcentre
            WHEN 'VSD GB'::text THEN max(COALESCE((tbltestrequirementrccost.price)::numeric, qrytestpcccostplan.price))
            ELSE (0)::numeric
        END AS vetr,
        CASE qrytestpcccostplan.profitcentre
            WHEN 'Viro'::text THEN max(COALESCE((tbltestrequirementrccost.price)::numeric, qrytestpcccostplan.price))
            ELSE (0)::numeric
        END AS viro
   FROM (fps.qrytestpcccostplan
     LEFT JOIN fps.tbltestrequirementrccost ON ((((qrytestpcccostplan.profitcentre)::text = (tbltestrequirementrccost.profitcentre)::text) AND ((qrytestpcccostplan.jobcode)::text = (tbltestrequirementrccost.buyer)::text) AND ((qrytestpcccostplan.testcode)::text = (tbltestrequirementrccost.testcode)::text) AND (qrytestpcccostplan.fpsyear = tbltestrequirementrccost.fpsyear))))
  GROUP BY qrytestpcccostplan.jobcode, qrytestpcccostplan.testcode, qrytestpcccostplan.profitcentre, qrytestpcccostplan.fpsyear
  ORDER BY qrytestpcccostplan.jobcode;
