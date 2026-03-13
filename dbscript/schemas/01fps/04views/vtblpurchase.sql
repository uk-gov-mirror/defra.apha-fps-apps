-- View: fps.vtblpurchase

CREATE OR REPLACE VIEW fps.vtblpurchase AS
SELECT DISTINCT
    tp.workgroup,
    tp.account,
    tp.itemdescription,
    tp.amount,
    tp.fpsyear,
    u.dt2username,
    u.useremail
FROM fps.tblpurchase tp
JOIN fps.tblbid b                 ON tp.workgroup = b.workgroup
JOIN fps.workgroup w              ON b.workgroup  = w.workgroup
JOIN fps.tblkpprofitcentre pc     ON w.profitcentre = pc.profitcentre
JOIN fps.tbluser_profitcentre upc ON pc.profitcentre = upc.profitcentre
JOIN fps.tblusers u               ON upc.user_id = u.user_id
WHERE tp.account IN (
    SELECT b2.account
    FROM fps.tblbid b2
    JOIN fps.workgroup w2              ON b2.workgroup   = w2.workgroup
    JOIN fps.tblkpprofitcentre pc2     ON w2.profitcentre = pc2.profitcentre
    JOIN fps.tbluser_profitcentre upc2 ON pc2.profitcentre = upc2.profitcentre
    WHERE upc2.user_id = u.user_id
);
