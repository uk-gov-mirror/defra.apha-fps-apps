-- View: fps.vqrytbidsum

CREATE OR REPLACE VIEW fps.vqrytbidsum AS
SELECT
    pc.profitcentre,
    b.fpsyear,
    sum(b.genbid)       AS sumofgenbid,
    u.dt2username,
    u.useremail
FROM fps.tblkpprofitcentre pc
JOIN fps.workgroup w              ON pc.profitcentre = w.profitcentre
JOIN fps.tblbid b                 ON w.workgroup = b.workgroup
                                 AND w.fpsyear   = b.fpsyear
JOIN fps.tbluser_profitcentre upc ON pc.profitcentre = upc.profitcentre
JOIN fps.tblusers u               ON upc.user_id = u.user_id
GROUP BY pc.profitcentre, b.fpsyear, u.dt2username, u.useremail;
