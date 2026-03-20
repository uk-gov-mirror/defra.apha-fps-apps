-- View: fps.vworkgroup

CREATE OR REPLACE VIEW fps.vworkgroup AS
SELECT DISTINCT
    w.workgroup,
    w.profitcentre,
    w.costcentre,
    w.owner,
    w.description,
    w.centraloverhead,
    w.sendemail,
    w.cos90,
    w.costcentreold,
    w.email_recipient,
    w.fpsyear,
    vpc.user_id,
    vpc.dt2username,
    vpc.useremail
FROM fps.workgroup w
JOIN fps.vtblkpprofitcentre vpc ON w.profitcentre = vpc.profitcentre;
