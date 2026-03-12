-- View: fps.vprojectstaffcount

CREATE OR REPLACE VIEW fps.vprojectstaffcount AS
 SELECT jobcode,
    fpsyear,
    count(staffid) AS countofstaff
   FROM fps.tblstaffjob
  GROUP BY jobcode, fpsyear;
