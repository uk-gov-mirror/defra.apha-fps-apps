-- View: fps.vprofitcentregrade

CREATE OR REPLACE VIEW fps.vprofitcentregrade AS
SELECT DISTINCT
    pcg.pcgrade,
    pcg.divisiongrade,
    pcg.gradecode,
    pcg.profitcentre,
    pcg.chargerate,
    pcg.directrate,
    pcg.payrate,
    pcg.npr,
    pcg.ohr,
    pcg.hrsavailable,
    pcg.oldchargerate,
    pcg.defrachargerate,
    pcg.fpsyear,
    vpc.user_id,
    vpc.dt2username,
    vpc.useremail
FROM fps.profitcentregrade pcg
JOIN fps.vtblkpprofitcentre vpc ON pcg.profitcentre = vpc.profitcentre;
