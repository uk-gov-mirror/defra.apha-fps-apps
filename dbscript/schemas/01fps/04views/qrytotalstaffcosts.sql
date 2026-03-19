-- View: fps.qrytotalstaffcosts

CREATE OR REPLACE VIEW fps.qrytotalstaffcosts AS
 SELECT DISTINCT parentproject AS jobcode,
    fpsyear,
    sum(cost) AS totalstaffcosts,
    sum(paycost) AS totalpaycosts
   FROM fps.vprojectstaffplan
  GROUP BY parentproject, fpsyear;
