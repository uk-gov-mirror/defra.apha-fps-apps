-- View: fps.vtblbid

CREATE OR REPLACE VIEW fps.vtblbid AS
 SELECT b.workgroup,
    b.account,
    b.genbid,
    b.fpsyear
   FROM (fps.tblbid b
     JOIN fps.vworkgroup wg ON (((wg.workgroup = b.workgroup) AND (wg.fpsyear = b.fpsyear))));
