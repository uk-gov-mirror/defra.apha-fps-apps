-- View: fps.vtblkpprofitcentre

CREATE OR REPLACE VIEW fps.vtblkpprofitcentre AS
SELECT DISTINCT
    pc.profitcentre,
    pc.profitcentrename,
    pc.division,
    pc.conttarget,
    pc.profitcentrehead,
    pc.divisionid,
    pc.email_recipient,
    pc.highlevelsummary,
    u.dt2username,
    u.useremail
FROM fps.tblkpprofitcentre pc
JOIN fps.tbluser_profitcentre upc ON pc.profitcentre = upc.profitcentre
JOIN fps.tblusers u               ON upc.user_id = u.user_id;
