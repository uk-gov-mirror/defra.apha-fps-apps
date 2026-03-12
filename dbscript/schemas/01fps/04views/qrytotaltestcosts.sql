-- View: fps.qrytotaltestcosts

CREATE OR REPLACE VIEW fps.qrytotaltestcosts AS
SELECT DISTINCT jobcode,
    fpsyear,
    sum((notests * testprice)) AS totaltestcosts
   FROM fps.vtbltestrequ
  GROUP BY jobcode, fpsyear;
