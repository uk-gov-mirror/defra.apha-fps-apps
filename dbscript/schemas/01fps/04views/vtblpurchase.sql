-- View: fps.vtblpurchase

CREATE OR REPLACE VIEW fps.vtblpurchase AS
 SELECT workgroup,
    account,
    itemdescription,
    amount,
    fpsyear
   FROM fps.tblpurchase
  WHERE (((workgroup)::text IN ( SELECT tblbid.workgroup
           FROM fps.tblbid
          WHERE ((tblbid.workgroup)::text IN ( SELECT workgroup.workgroup
                   FROM fps.workgroup
                  WHERE ((workgroup.profitcentre)::text IN ( SELECT tblkpprofitcentre.profitcentre
                           FROM fps.tblkpprofitcentre
                          WHERE ((tblkpprofitcentre.profitcentre)::text IN ( SELECT tbluser_profitcentre.profitcentre
                                   FROM fps.tbluser_profitcentre
                                  WHERE (tbluser_profitcentre.user_id IN ( SELECT tblusers.user_id
   FROM fps.tblusers
  WHERE ((tblusers.dt2username)::text = CURRENT_USER))))))))))) AND ((account)::text IN ( SELECT tblbid.account
           FROM fps.tblbid
          WHERE ((tblbid.workgroup)::text IN ( SELECT workgroup.workgroup
                   FROM fps.workgroup
                  WHERE ((workgroup.profitcentre)::text IN ( SELECT tblkpprofitcentre.profitcentre
                           FROM fps.tblkpprofitcentre
                          WHERE ((tblkpprofitcentre.profitcentre)::text IN ( SELECT tbluser_profitcentre.profitcentre
                                   FROM fps.tbluser_profitcentre
                                  WHERE (tbluser_profitcentre.user_id IN ( SELECT tblusers.user_id
   FROM fps.tblusers
  WHERE ((tblusers.dt2username)::text = CURRENT_USER))))))))))));
