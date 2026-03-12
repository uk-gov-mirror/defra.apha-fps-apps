-- View: fps.vprofitcentregrade

CREATE OR REPLACE VIEW fps.vprofitcentregrade AS
 SELECT pcgrade,
    divisiongrade,
    gradecode,
    profitcentre,
    chargerate,
    directrate,
    payrate,
    npr,
    ohr,
    hrsavailable,
    oldchargerate,
    defrachargerate,
    fpsyear
   FROM fps.profitcentregrade
  WHERE ((profitcentre)::text IN ( SELECT vtblkpprofitcentre.profitcentre
           FROM fps.vtblkpprofitcentre));
