ARG IMAGE_TAG="latest-x86_64"

FROM public.ecr.aws/sam/build-dotnet8:${IMAGE_TAG}

ARG ARCH="linux-x64"

WORKDIR /var/task

COPY ["Catnekaise.Ghrawel.TokenProvider/Catnekaise.Ghrawel.TokenProvider.csproj", "Catnekaise.Ghrawel.TokenProvider/"]
RUN dotnet restore --runtime ${ARCH} -v d ./Catnekaise.Ghrawel.TokenProvider/

COPY Catnekaise.Ghrawel.TokenProvider/. ./Catnekaise.Ghrawel.TokenProvider/

RUN dotnet publish ./Catnekaise.Ghrawel.TokenProvider \
    --no-restore --configuration "Release" --framework "net8.0" \
    --runtime ${ARCH} \
    -o /asset /p:SelfContained=true /p:GenerateRuntimeConfigurationFiles=true