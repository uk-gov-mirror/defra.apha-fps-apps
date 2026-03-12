-- View: fps.vpacttblstaff

CREATE OR REPLACE VIEW fps.vpacttblstaff AS
 SELECT tblwgemployee.pactid,
    tblemployee.spnumber,
    (((COALESCE(tblemployee.lastname, ''::character varying))::text || ', '::text) || (COALESCE(tblemployee.firstname, ''::character varying))::text) AS name,
    tblwgemployee.workgroupgrade,
    tblemployee.title,
    tblwgemployee.personstatus,
    tblwgemployee.personclass,
    tblwgemployee.hrspaid,
    tblwgemployee.leave,
    tblwgemployee.sickspecial,
    tblwgemployee.hrsavail,
    tblwgemployee.fpsyear
   FROM (fps.tblemployee
     JOIN fps.tblwgemployee ON (((tblemployee.spnumber)::text = (tblwgemployee.spnumber)::text)));
