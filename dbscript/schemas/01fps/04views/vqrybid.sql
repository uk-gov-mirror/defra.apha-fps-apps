-- View: fps.vqrybid

CREATE OR REPLACE VIEW fps.vqrybid AS
 SELECT DISTINCT tblkpaccountcategory.accshortname,
    tblbid.workgroup,
    tblbid.genbid,
    workgroup.profitcentre,
    tblbid.fpsyear
   FROM ((fps.tblkpaccountcategory
     LEFT JOIN fps.tblbid ON (((tblkpaccountcategory.accshortname)::text = (tblbid.account)::text)))
     LEFT JOIN fps.workgroup ON ((((tblbid.workgroup)::text = (workgroup.workgroup)::text) AND (tblbid.fpsyear = workgroup.fpsyear))))
  GROUP BY tblkpaccountcategory.accshortname, tblbid.workgroup, tblbid.genbid, workgroup.profitcentre, tblbid.fpsyear;
