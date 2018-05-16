# To update this docker image run:
# ```
# cd continuous-integration/docker
# docker build -t dam5s/somanyfeeds-ci .
# docker push dam5s/somanyfeeds-ci
# ```

FROM microsoft/dotnet:2.0.7-sdk-2.1.200-jessie

ENV DEBIAN_FRONTEND noninteractive

RUN apt-get update && apt-get -y install --no-install-recommends wget

RUN wget https://dl.bintray.com/elmlang/elm-platform/0.18.0/linux-x64.tar.gz \
    && mkdir -p elm \
    && tar xzf linux-x64.tar.gz -C elm \
    && cp elm/dist_binaries/* /bin/ \
    && rm -rf elm

ADD build build
