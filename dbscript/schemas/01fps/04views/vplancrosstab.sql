-- View: fps.vplancrosstab

CREATE OR REPLACE VIEW fps.vplancrosstab AS
SELECT jobcode,
    testcode,
    fpsyear,
    sum(labt) AS labt,
    sum(vetr) AS vetr,
    sum(viro) AS viro
   FROM fps.qrytestspccostplan_crosstab
  GROUP BY jobcode, testcode, fpsyear;
