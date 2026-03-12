-- View: fps.vqrytbidsum
CREATE OR REPLACE VIEW fps.vqrytbidsum AS
 SELECT tblkpprofitcentre.profitcentre,
    tblbid.fpsyear,
    sum(tblbid.genbid) AS sumofgenbid
   FROM (fps.tblkpprofitcentre
     JOIN (fps.workgroup
     JOIN fps.tblbid ON (((workgroup.workgroup)::text = (tblbid.workgroup)::text) AND workgroup.fpsyear = tblbid.fpsyear)) ON (((tblkpprofitcentre.profitcentre)::text = (workgroup.profitcentre)::text)))
  WHERE ((tblkpprofitcentre.profitcentre)::text IN ( SELECT tbluser_profitcentre.profitcentre
           FROM fps.tbluser_profitcentre
          WHERE (tbluser_profitcentre.user_id IN ( SELECT tblusers.user_id
                   FROM fps.tblusers
                  WHERE ((tblusers.dt2username)::text = CURRENT_USER)))))
  GROUP BY tblkpprofitcentre.profitcentre, tblbid.fpsyear;
