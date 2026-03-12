-- View: fps.qrytestpcccostplan

CREATE OR REPLACE VIEW fps.qrytestpcccostplan AS
SELECT vtbltestrequ_tm.jobcode,
    vtbltestrequ_tm.testcode,
    tbltestrccost.profitcentre,
    (tbltestrccost.price)::numeric AS price,
    vtbltestrequ_tm.fpsyear
   FROM (fps.vtbltestrequ_tm
     LEFT JOIN fps.tbltestrccost ON (((vtbltestrequ_tm.testcode)::text = (tbltestrccost.testcode)::text) AND vtbltestrequ_tm.fpsyear = tbltestrccost.fpsyear))
  WHERE ((tbltestrccost.price IS NOT NULL) AND (tbltestrccost.profitcentre IS NOT NULL))
  ORDER BY vtbltestrequ_tm.jobcode;
