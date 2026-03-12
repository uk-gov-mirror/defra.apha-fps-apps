-- View: fps.vtblwgemployee_general

CREATE OR REPLACE VIEW fps.vtblwgemployee_general AS
 SELECT pactid,
    spnumber,
    workgroupgrade,
    fpsyear
   FROM fps.tblwgemployee;
