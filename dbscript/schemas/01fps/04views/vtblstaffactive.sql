-- View: fps.vtblstaffactive

CREATE OR REPLACE VIEW fps.vtblstaffactive AS
 SELECT tblwgemployee.pactid AS staffid,
    (((COALESCE(tblemployee.lastname, ''::character varying))::text || ', '::text) || (COALESCE(tblemployee.firstname, ''::character varying))::text) AS name,
    tblwgemployee.workgroupgrade,
    tblwgemployee.fpsyear
   FROM fps.tblwgemployee,
    fps.tblemployee
  WHERE (((tblwgemployee.spnumber)::text = (tblemployee.spnumber)::text) AND ((tblwgemployee.personstatus)::text = 'A'::text));
