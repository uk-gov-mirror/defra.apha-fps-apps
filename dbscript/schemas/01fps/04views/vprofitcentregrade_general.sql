-- View: fps.vprofitcentregrade_general

CREATE OR REPLACE VIEW fps.vprofitcentregrade_general AS
 SELECT pcgrade,
    divisiongrade,
    gradecode,
    profitcentre,
    chargerate,
    defrachargerate,
    fpsyear
   FROM fps.profitcentregrade;
